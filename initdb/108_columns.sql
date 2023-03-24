create table columns (
    id bigserial primary key,
    board_id bigint references boards(id) on delete cascade on update cascade,
    position int not null,
    name varchar(50) not null
)