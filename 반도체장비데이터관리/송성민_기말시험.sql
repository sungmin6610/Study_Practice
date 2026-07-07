use Processsensordb;

#1-1
create table AlarmLog(
alarm_id int,
equip_id int not null,
run_id int not null,
alarm_time datetime not null,
alarm_level varchar(20) not null,
alarm_message varchar(255) not null,
primary key(alarm_id),
foreign key(equip_id) references equipment(equipment_id) on delete cascade,
foreign key(run_id) references RunHistory(run_id) on delete cascade);

#1-2
insert into AlarmLog values(1, 101, 7, '2024-03-15 14:20:00', 'WARNING', '챔버 온도 정상 범위 초과');
insert into AlarmLog values(2, 101, 7, '2024-03-15 14:30:00', 'CRITICAL', '챔버 온도 지속 상승');
insert into AlarmLog values(3, 102, 8, '2024-03-15 15:05:00', 'WARNING', 'RF 출력 정상 범위 초과');
insert into AlarmLog values(4, 102, 8, '2024-03-15 15:05:00', 'WARNING', '가스 유량 정상 범위 이하');
insert into AlarmLog values(5, 102, 8, '2024-03-15 15:20:00', 'CRITICAL', 'RF 출력 및 가스 유량 이상 상태 지속');

#2-1
select e.equipment_id, e.model_name, e.status, count(a.alarm_id)
from AlarmLog as a
join equipment as e on a.equip_id = e.equipment_id
group by e.equipment_id, e.model_name;

#3-1
select alarm_level, count(alarm_id) as '알람 발생 횟수'
from AlarmLog
group by alarm_level;

#4-1
select r.run_id, e.equipment_id, s.sensor_name, sm.measured_value, s.normal_min, s.normal_max, a.alarm_level, a.alarm_message
from sensormeasurement as sm
join sensor as s on sm.sensor_id = s.sensor_id
join runhistory as r on s.equipment_id = r.equipment_id
join alarmlog as a on r.run_id = a.run_id
where tmeasured_value is not null;

#5-1
select r.run_id, r.equipment_id, r.run_status, count(alarm_id) as '알람 발생 횟수'
from runhistory as r
join alarmlog as a on r.run_id = a.run_id
group by r.run_id
order by count(alarm_id);



