-- Postgres initial schema (idempotent) generated from SQL Server migrations
-- NOTE: This script is a best-effort translation for initial setup on Postgres.
-- Run in pre-deploy step. Review before applying in production.

BEGIN;

CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
  MigrationId varchar(150) NOT NULL PRIMARY KEY,
  ProductVersion varchar(32) NOT NULL
);

-- Advertisements
CREATE TABLE IF NOT EXISTS advertisements (
  id uuid NOT NULL,
  publicid text NOT NULL,
  title text NOT NULL,
  imageurl text NOT NULL,
  type text NOT NULL,
  sortorder integer NOT NULL,
  createdat timestamp without time zone NOT NULL,
  PRIMARY KEY (id)
);

-- Announcements
CREATE TABLE IF NOT EXISTS announcements (
  id uuid NOT NULL,
  classname text NOT NULL,
  isactive boolean NOT NULL,
  createdat timestamp without time zone NOT NULL,
  updatedat timestamp without time zone NOT NULL,
  PRIMARY KEY (id)
);

-- Categories
CREATE TABLE IF NOT EXISTS categories (
  categoryid uuid NOT NULL,
  name text NOT NULL,
  parentid uuid NULL,
  createdat timestamp without time zone NOT NULL,
  imageurl text NULL,
  icon text NULL,
  PRIMARY KEY (categoryid),
  CONSTRAINT fk_categories_parent FOREIGN KEY (parentid) REFERENCES categories(categoryid)
);

-- Coupons
CREATE TABLE IF NOT EXISTS coupons (
  couponid uuid NOT NULL,
  code text NOT NULL,
  discountpercent numeric(10,2) NOT NULL,
  minorderamount numeric(18,2) NULL,
  maxdiscountamount numeric(18,2) NULL,
  startdate timestamp without time zone NOT NULL,
  enddate timestamp without time zone NOT NULL,
  usagelimit integer NULL,
  usedcount integer NOT NULL,
  isactive boolean NOT NULL,
  PRIMARY KEY (couponid)
);

-- Users (minimal)
CREATE TABLE IF NOT EXISTS users (
  id uuid NOT NULL,
  fullname text NOT NULL,
  email text NOT NULL,
  passwordhash text NOT NULL,
  phone varchar(450) NULL,
  role text NOT NULL,
  gender text NULL,
  avatar text NULL,
  googleid text NULL,
  emailverified boolean NOT NULL,
  mfaenabled boolean NOT NULL,
  mfatype text NULL,
  createdat timestamp without time zone NOT NULL,
  updatedat timestamp without time zone NULL,
  PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_users_phone ON users (phone) WHERE phone IS NOT NULL;

-- Products and related
CREATE TABLE IF NOT EXISTS products (
  productid uuid NOT NULL,
  name text NOT NULL,
  description text NULL,
  additionalinfo text NULL,
  gendertarget text NULL,
  brand text NULL,
  averagerating numeric(5,2) NOT NULL,
  totalreviews integer NOT NULL,
  totallikes integer NOT NULL,
  createdat timestamp without time zone NOT NULL,
  updatedat timestamp without time zone NULL,
  isactive boolean NOT NULL,
  tagsjson text NULL,
  PRIMARY KEY (productid)
);

CREATE TABLE IF NOT EXISTS productvariants (
  variantid uuid NOT NULL,
  productid uuid NOT NULL,
  sku text NOT NULL,
  baseprice numeric(18,0) NOT NULL,
  discountpercent numeric(10,2) NOT NULL,
  discountamount numeric(18,0) NOT NULL,
  priceafterdiscount numeric(18,0) NOT NULL,
  stockquantity integer NOT NULL,
  isdefault boolean NOT NULL,
  displayorder integer NOT NULL,
  imageurl text NULL,
  imghover text NULL,
  thumbnailurl text NULL,
  isactive boolean NOT NULL,
  PRIMARY KEY (variantid),
  CONSTRAINT fk_productvariants_product FOREIGN KEY (productid) REFERENCES products(productid) ON DELETE CASCADE
);

-- Attribute tables
CREATE TABLE IF NOT EXISTS productattributes (
  attributeid uuid NOT NULL,
  name text NOT NULL,
  inputtype text NULL,
  isactive boolean NOT NULL,
  PRIMARY KEY (attributeid)
);

CREATE TABLE IF NOT EXISTS attributevalues (
  attributevalueid uuid NOT NULL,
  attributeid uuid NOT NULL,
  value text NOT NULL,
  displayorder integer NOT NULL,
  isactive boolean NOT NULL,
  PRIMARY KEY (attributevalueid),
  CONSTRAINT fk_attributevalues_attribute FOREIGN KEY (attributeid) REFERENCES productattributes(attributeid) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS categoryattributes (
  categoryid uuid NOT NULL,
  attributeid uuid NOT NULL,
  assignedat timestamp without time zone NOT NULL,
  PRIMARY KEY (categoryid, attributeid),
  CONSTRAINT fk_categoryattributes_category FOREIGN KEY (categoryid) REFERENCES categories(categoryid) ON DELETE CASCADE,
  CONSTRAINT fk_categoryattributes_attribute FOREIGN KEY (attributeid) REFERENCES productattributes(attributeid) ON DELETE CASCADE
);

-- Addresses, Carts, Orders simplified
CREATE TABLE IF NOT EXISTS addresses (
  addressid uuid NOT NULL,
  userid uuid NOT NULL,
  recipientname text NOT NULL,
  fulladdress text NOT NULL,
  latitude double precision NULL,
  longitude double precision NULL,
  isdefault boolean NOT NULL,
  PRIMARY KEY (addressid),
  CONSTRAINT fk_addresses_user FOREIGN KEY (userid) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS carts (
  cartid uuid NOT NULL,
  userid uuid NOT NULL,
  createdat timestamp without time zone NOT NULL,
  PRIMARY KEY (cartid),
  CONSTRAINT fk_carts_user FOREIGN KEY (userid) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS orders (
  orderid uuid NOT NULL,
  userid uuid NOT NULL,
  orderdate timestamp without time zone NOT NULL,
  status text NOT NULL,
  cancelreason text NULL,
  admincancelreason text NULL,
  cancelledby text NULL,
  cancelledat timestamp without time zone NULL,
  subtotal numeric(18,0) NOT NULL,
  discountamount numeric(18,0) NOT NULL,
  totalamount numeric(18,0) NOT NULL,
  buyername text NULL,
  buyerphone text NULL,
  shippingaddress text NULL,
  shippinglat double precision NULL,
  shippinglng double precision NULL,
  PRIMARY KEY (orderid),
  CONSTRAINT fk_orders_user FOREIGN KEY (userid) REFERENCES users(id) ON DELETE CASCADE
);

-- Minimal indexes for performance (add more as needed)
CREATE INDEX IF NOT EXISTS ix_addresses_userid ON addresses (userid);
CREATE INDEX IF NOT EXISTS ix_productvariants_productid ON productvariants (productid);

-- Record this migration as applied
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260128042615_InitCreate_Postgres', '9.0.9') ON CONFLICT DO NOTHING;

COMMIT;
