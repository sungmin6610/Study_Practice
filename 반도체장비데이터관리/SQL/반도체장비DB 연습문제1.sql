# 연습문제 1. Semicon_equipDB
use Semicon_EquipDB;

#2
select * from Equipment where equipment_id order by install_date desc limit 1;

#3
select use_date, equipment_id from UsageLog where use_date and Equipment_id IS NOT NULL order by use_date desc limit 1;

#4
select * from UsageLog where use_date IS NOT NULL order by use_date desc;

#5
select equipment_id, install_date from Equipment where status <> 'active';

#6
select * from equipment where model_name like "%CVD%";

#7
select equipment_id from equipment where status = "active" and install_date >= '2022-01-01';

#8
select equipment_id from UsageLog where use_date between "2024-03-10" and "2024-03-15";

#9
select model_name from equipment where status = "active";

#10
select * from UsageLog where equipment_id = 103 order by use_date desc limit 1;