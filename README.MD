src/
├── domain/                 # Lõi nghiệp vụ thuần
│   ├── entities/
│   ├── valueObjects/
│   ├── events/
│   ├── repositories/      # Interfaces/Ports
│   └── services/          # Domain services
│
├── application/           # Use cases + business logic
│   ├── usecases/
│   ├── dtos/
│   ├── interfaces/        # Abstraction để infra implement
│   └── validators/        # Validation logic, nếu cần
│
├── infrastructure/        # Chi tiết implement
│   ├── db/                # DB drivers, ORM, migrations
│   ├── services/          # Client 3rd-party, provider
│   └── repositories/      # Implementation của Interfaces
│
├── shared/                # Code dùng chung (utils/types/constants)
│   ├── constants/
│   ├── errors/
│   ├── utils/
│   └── types/
│
└── tests/                 # Test từng layer riêng như Domain, Application, Infra