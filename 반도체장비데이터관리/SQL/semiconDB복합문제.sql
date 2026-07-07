#복합 연습문제

use semicon_equipDB;

#1
select * from Equipment where status = 'active' and install_date > '2022-01-01';

#2
select * from EquipmentUser where department = '연구팀' or '품질팀';

#3
select * from UsageLog where use_date between '2024-03-05' and '2024-03-12' and issue_report IS NOT NULL order by use_date;

#4
select * from Equipment where model_name ('%A%') and status not in ('retired');

#5
select * from EquipmentUser where name like '김%' or name like '정%' order by name asc;

#6
select * from UsageLog where equipment_id = '101' or equipment_id = '103' and use_date > 2024-03-02 order by use_date asc;

#7
select * from Equipment where status = 'active' or 'maintanance' and install-date between 2021-01-01 and 2023-12-31;

#8
select * from UsageLog where issue_report like '%이상%' order by use_date asc;

#9
select * from EquipmentUser where department <> '연구팀' and (name like '%아%' or name like '%훈%');

#10
select * from Equipment where model_name like 'ETCH%' or model_name like 'CVD%' order by install_date;

#11
select * from UsageLog where issue_report IS NULL and equipment_id in('102', '103', '105') order by use_date;

#12
select distinct department from EquipmentUser order by department asc;

#13
select * from Equipment where status = 'active' and model_name like('%-%') order by install_date desc;

#14
select * from UsageLog where use_date >= '2024-03-08' or issue_report IS NOT NULL order by use_date;

#15
select distinct equipment_id from UsageLog where use_date >= '2024-03-05';

#응용연습문제

#1. 가장 최근에 사용된 장비는?
select * from UsageLog where use_date order by use_date desc;

#2. 가장 오래된 장비는?
select * from equipment where install_date order by install_date;

#3. 가장 최근에 발생한 문제는 무엇인가?
select * from usagelog where issue_report IS NOT NULL order by use_date desc;

#4. 가장 최근에 문제가 발생한 장비는 무엇인가?
select * from usagelog where issue_report