create table tasks (
    id bigserial,
    column_id bigint not null,

    position int not null,
    name varchar(50) not null,
    description text not null,
    creation_time date not null,
    status varchar(20) not null,
    priority int not null default(1),
    id_executor bigint,
    deadline date,
    estimated_time interval,

    primary key (id),

    foreign key (column_id) references columns(id)
        on delete cascade on update cascade,

    foreign key (id_executor) references accounts(id)
        on delete no action on update cascade
)