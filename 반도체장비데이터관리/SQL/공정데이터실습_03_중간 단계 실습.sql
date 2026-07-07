-- =========================================================
-- ProcessSensorDB 중간 단계 실습
-- 목표:
-- 1. 두 개 이상의 테이블을 연결한다.
-- 2. 데이터가 어느 테이블에 기록되어 있는지 감각을 익힌다.
-- 3. 해석 문제로 넘어가기 전 JOIN과 GROUP BY를 연습한다.
-- =========================================================

USE ProcessSensorDB;

select * 
from processstep;

select *
from Lot;

select *
from equipment;

select *
from runhistory;

select *
from sensor;

select *
from sensormeasurement;

-- 문제 1
-- Lot L001은 어떤 공정 단계를 거쳤는가?
-- 힌트:
-- - RunHistory에 공정 실행 기록이 있음
-- - 공정 이름은 ProcessStep에 있음
select *
from runhistory 
join processstep on runhistory.step_id = processstep.step_id
where runhistory.lot_id = 'L001';

-- 문제 2
-- Run 7은 어떤 장비에서 수행되었는가?
-- 힌트:
-- - RunHistory와 Equipment를 연결해야 함
select model_name
from runhistory
join equipment on runhistory.equipment_id = equipment.equipment_id
where run_id = '7'; 

-- 문제 3
-- Run 7에서 측정된 센서 이름과 값을 확인하세요.
-- 힌트:
-- - 측정값은 SensorMeasurement에 있음
-- - 센서 이름은 Sensor에 있음
select sensor_name, measured_value
from sensormeasurement
join sensor on sensormeasurement.sensor_id = sensor.sensor_id
where run_id = 7;

-- 문제 4
-- ETCH-A100 장비에는 어떤 센서들이 달려 있는가?
-- 힌트:
-- - 장비 정보는 Equipment에 있음
-- - 센서 정보는 Sensor에 있음
select sensor_name, sensor_id
from equipment
join sensor on equipment.equipment_id = sensor.equipment_id
where model_name = 'ETCH-A100';

-- 문제 5
-- warning 상태인 Run은 어떤 Lot에서 발생했는가?
-- 힌트:
-- - Run의 상태는 RunHistory에 있음
-- - Lot의 제품 정보는 Lot에 있음
select run_id
from runhistory
join lot on runhistory.lot_id = lot.lot_id
where lot_status = 'warning';

-- 문제 6
-- Run 7의 센서값을 시간 순으로 확인하세요.
-- 힌트:
-- - SensorMeasurement 테이블을 사용
-- - measured_at 기준으로 정렬
select measured_value
from sensormeasurement
where run_id = '7'
order by measured_at;

-- 문제 7
-- 센서값과 정상 범위를 함께 확인하세요.
-- 힌트:
-- - 측정값은 SensorMeasurement에 있음
-- - 정상 범위는 Sensor에 있음
select measured_value, normal_min, normal_max
from sensormeasurement
join sensor on sensormeasurement.sensor_id = sensor.sensor_id
where measured_value > normal_min and measured_value < normal_max; 

-- 문제 8
-- 정상 범위를 벗어난 센서값만 찾아보세요.
-- 힌트:
-- - measured_value가 normal_min보다 작거나
--   normal_max보다 큰 경우를 찾으면 됨
select measured_value
from sensormeasurement
join sensor on sensormeasurement.sensor_id = sensor.sensor_id
where measured_value < normal_min or measured_value > normal_max; 

-- 문제 9
-- Run별로 몇 개의 센서값이 기록되었는지 확인하세요.
-- 힌트:
-- - SensorMeasurement 사용
-- - GROUP BY 사용
select run_id, count(measured_value)
from sensormeasurement
group by run_id;

-- 문제 10
-- Lot L003의 모든 Run과 해당 장비를 확인하세요.
-- 힌트:
-- - RunHistory와 Equipment를 연결
-- - Lot 조건은 lot_id = 'L003'
select  *
from runhistory 
join equipment on runhistory.equipment_id = equipment.equipment_id
where lot_id = 'L003';