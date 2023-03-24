create table tasks (
    id bigserial primary key,
    column_id bigint references columns(id) on delete cascade on update cascade,

    position int not null,
    name varchar(50) not null,
    description text not null,
    creation_time date not null,
    status varchar(20) not null,
    priority int not null default(1),
    id_executor bigint references accounts(id) on delete no action on update cascade,
    deadline date not null,
    estimated_time int not null default(0)
)