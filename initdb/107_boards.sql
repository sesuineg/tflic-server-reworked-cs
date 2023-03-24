create table boards (
    id bigserial primary key,
    project_id bigint references projects(id) on delete cascade on update cascade,
    name varchar(50) not null
)