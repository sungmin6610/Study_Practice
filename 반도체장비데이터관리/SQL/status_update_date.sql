use semicon_equipDB;
select * from department;
select * from employee;
#문제 1-1
rename table equipmentuser
to employee;

select * from
employee;

desc employee;

select * from
usagelog;

select * from
maintenancelog;

desc maintenancelog;

#문제 4-5
alter table maintenancelog
modify engineer_id int not null; 

#문제 5-1
alter table department
add manager_id int;


#문제 5-2
update department as d
join employee as e on d.manager_name = e.name
set manager_id = employee_id;

#문제 5-3
select * 
from department
where manager_id is null;

#문제 5-4
alter table department
add constraint fk_manager_id
foreign key (manager_id)
references employee(employee_id);

#문제 5-5
alter table department
drop column manager_name;
select *
from department;

#문제 6-1
desc department;

#문제 6-2
show create table department;

#문제 6-3


ALTER TABLE Equipment
ADD status_update_date DATE;

UPDATE Equipment SET status_update_date = '2024-03-07' WHERE equipment_id = 101;
UPDATE Equipment SET status_update_date = '2024-03-06' WHERE equipment_id = 102;
UPDATE Equipment SET status_update_date = '2024-03-12' WHERE equipment_id = 103;
UPDATE Equipment SET status_update_date = '2024-03-09' WHERE equipment_id = 104;
UPDATE Equipment SET status_update_date = '2024-03-08' WHERE equipment_id = 105;