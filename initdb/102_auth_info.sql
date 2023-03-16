create table auth_info ( -- todo вынести в отдельную бд
    account_id bigint,
    login varchar(50) unique not null,
    password_hash varchar(44) not null,
    refresh_token varchar(44), -- todo вынести в отдельную таблицу
    refresh_token_expiration_time timestamp,
    
    primary key (account_id, login),
    foreign key (account_id) references accounts (id) on delete cascade on update cascade
)