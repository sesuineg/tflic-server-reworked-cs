create table accounts (
    id bigserial unique,
    name varchar(50) not null,

    primary key (id)
)