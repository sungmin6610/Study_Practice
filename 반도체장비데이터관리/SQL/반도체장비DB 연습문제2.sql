# 연습문제 2. Semicon_equipDB

use Semicon_EquipDB;

#1
select model_name as '장비모델', status as '장비상태' from equipment;

#2
select name as '사용자명', department as '소속부서' from equipmentuser;

#3
select equipment_id , use_date as '사용일자' from usagelog;


#1
select count(*) from equipment;

#2
select count(*) from usagelog;

#3
select max(use_date) from usagelog;

#4
select min(use_date) from usagelog;

#5
select max(install_date) from equipment;


#1
select count(*) from equipment where status = 'active';

#2
select count(*) from usagelog where issue_report is not null;

#3
select count(*) from usagelog where use_date >= '2024-03-10';

#4
select count(*) from usagelog where use_date >= '2024-03-10' and issue_report is not null;

#5
select max(use_date) from usagelog where equipment_id = '105';

#6
select count(*) from equipment where install_date >= '2022-01-01';

#7 
select count(distinct equipment_id) from usagelog;