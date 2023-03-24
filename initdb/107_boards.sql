create table boards (
    id bigserial,
    project_id bigint not null,
    name varchar(50) not null,

    primary key (id),

    foreign key (project_id) references projects(id)
        on delete cascade on update cascade
)
