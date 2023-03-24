create table projects (
    id bigserial,
    organization_id bigint not null,
    is_archived boolean default false,
    name varchar(50) not null,
    
    primary key (id),
        
    foreign key (organization_id) references organizations(id) 
        on delete cascade on update cascade
)
