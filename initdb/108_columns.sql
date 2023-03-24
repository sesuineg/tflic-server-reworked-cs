create table columns (
    id bigserial,
    board_id bigint not null,
    position int not null,
    name varchar(50) not null,

    primary key (id),

    foreign key (board_id) references boards(id)
        on delete cascade on update cascade
)
