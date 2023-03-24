create table projects (
    id bigserial primary key,
    organization_id bigint references organizations(id) on delete cascade on update cascade,
    is_archived boolean default false,
    name varchar(50) not null
)