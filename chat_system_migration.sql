-- ========================================================
-- CHAT SYSTEM MIGRATION SCRIPT
-- ========================================================
-- This script sets up the database schema for the real-time chat system.
-- Run this in your Supabase SQL Editor.

-- 1. CLEANUP (Uncomment to reset - WARNING: LOSES ALL CHAT DATA)
-- DROP TABLE IF EXISTS chat_messages;
-- DROP TABLE IF EXISTS chat_conversations;

-- 2. CREATE TABLES
CREATE TABLE IF NOT EXISTS chat_conversations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id TEXT NOT NULL UNIQUE, -- One conversation per user
    user_email TEXT NOT NULL,
    user_name TEXT NOT NULL,
    unread_count_by_user INTEGER DEFAULT 0,
    unread_count_by_admin INTEGER DEFAULT 0,
    last_message_at TIMESTAMPTZ DEFAULT now(),
    created_at TIMESTAMPTZ DEFAULT now()
);

CREATE TABLE IF NOT EXISTS chat_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id UUID REFERENCES chat_conversations(id) ON DELETE CASCADE,
    sender_id TEXT NOT NULL, -- User ID or 'admin'
    sender_role TEXT NOT NULL CHECK (sender_role IN ('user', 'admin')),
    sender_name TEXT NOT NULL,
    content TEXT NOT NULL,
    is_read BOOLEAN DEFAULT false,
    is_deleted BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ DEFAULT now()
);

-- 3. INDEXES FOR PERFORMANCE
CREATE INDEX IF NOT EXISTS idx_chat_messages_conversation_id ON chat_messages(conversation_id);
CREATE INDEX IF NOT EXISTS idx_chat_messages_created_at ON chat_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_chat_conversations_user_id ON chat_conversations(user_id);

-- 4. ENABLE REALTIME
-- Adds chat tables to the Supabase Realtime publication
ALTER PUBLICATION supabase_realtime ADD TABLE chat_conversations;
ALTER PUBLICATION supabase_realtime ADD TABLE chat_messages;

-- 5. ROW LEVEL SECURITY (RLS) - SECURE API PROXY MODE
ALTER TABLE chat_conversations ENABLE ROW LEVEL SECURITY;
ALTER TABLE chat_messages ENABLE ROW LEVEL SECURITY;

-- Policy: Allow admins or involved participants to read (SELECT).
-- Admins are identified via a JWT custom claim `role = 'admin'`.
-- Participants are the conversation owner (`chat_conversations.user_id`) or
-- the message sender (`chat_messages.sender_id`).
CREATE POLICY "select_conversation_admin_or_owner" ON chat_conversations FOR SELECT
USING (
    (current_setting('jwt.claims', true)::json ->> 'role') = 'admin'
    OR user_id = auth.uid()::text
);

CREATE POLICY "select_message_admin_or_participant" ON chat_messages FOR SELECT
USING (
    (current_setting('jwt.claims', true)::json ->> 'role') = 'admin'
    OR sender_id = auth.uid()::text
    OR EXISTS (
        SELECT 1 FROM chat_conversations cc WHERE cc.id = chat_messages.conversation_id AND cc.user_id = auth.uid()::text
    )
);

-- INSERT: allow authenticated users to insert messages as themselves, or admins.
CREATE POLICY "insert_message_sender_or_admin" ON chat_messages FOR INSERT
WITH CHECK (
    (current_setting('jwt.claims', true)::json ->> 'role') = 'admin'
    OR sender_id = auth.uid()::text
);

-- Optional: allow creating conversations when the user matches
CREATE POLICY "insert_conversation_owner_or_admin" ON chat_conversations FOR INSERT
WITH CHECK (
    (current_setting('jwt.claims', true)::json ->> 'role') = 'admin'
    OR user_id = auth.uid()::text
);

-- UPDATE/DELETE: restrict to admins only to simplify audit/cleanup.
CREATE POLICY "update_message_admin_only" ON chat_messages FOR UPDATE
USING ((current_setting('jwt.claims', true)::json ->> 'role') = 'admin');

CREATE POLICY "delete_message_admin_only" ON chat_messages FOR DELETE
USING ((current_setting('jwt.claims', true)::json ->> 'role') = 'admin');

CREATE POLICY "update_conversation_admin_only" ON chat_conversations FOR UPDATE
USING ((current_setting('jwt.claims', true)::json ->> 'role') = 'admin');

CREATE POLICY "delete_conversation_admin_only" ON chat_conversations FOR DELETE
USING ((current_setting('jwt.claims', true)::json ->> 'role') = 'admin');

-- 6. AUTOMATIC METADATA UPDATES
-- Trigger function to update 'last_message_at' whenever a new message is sent.
CREATE OR REPLACE FUNCTION update_last_message_at()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE chat_conversations
    SET last_message_at = NEW.created_at
    WHERE id = NEW.conversation_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_update_last_message_at ON chat_messages;
CREATE TRIGGER tr_update_last_message_at
AFTER INSERT ON chat_messages
FOR EACH ROW
EXECUTE FUNCTION update_last_message_at();

-- 7. AUTOMATED CHAT CLEANUP JOB (5-DAY RESET)
-- This background job deletes messages older than 5 days.
CREATE EXTENSION IF NOT EXISTS pg_cron;

CREATE OR REPLACE FUNCTION delete_old_chat_data()
RETURNS void AS $$
BEGIN
    DELETE FROM chat_messages
    WHERE created_at < NOW() - INTERVAL '5 days';

    DELETE FROM chat_conversations
    WHERE id NOT IN (SELECT DISTINCT conversation_id FROM chat_messages)
      AND last_message_at < NOW() - INTERVAL '5 days';
END;
$$ LANGUAGE plpgsql;

-- Schedule to run daily at midnight
SELECT cron.schedule(
    'daily-chat-cleanup',
    '0 0 * * *',
    'SELECT delete_old_chat_data();'
);

-- VERIFICATION
-- SELECT * FROM cron.job;
-- SELECT tablename, rowsecurity FROM pg_tables WHERE tablename LIKE 'chat_%';
-- SELECT * FROM pg_publication_tables WHERE pubname = 'supabase_realtime';

