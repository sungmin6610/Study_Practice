-- =========================================================
-- ProcessSensorDB 기초 SELECT 실습
-- 목표:
-- 테이블 이름, 컬럼 이름, 데이터의 의미에 익숙해지기
-- =========================================================
use ProcessSensorDB;

-- 1. 전체 데이터 구경하기
-- 문제 1
-- 공정 단계 목록을 모두 조회하세요.
select * 
from processstep;

-- 문제 2
-- Lot 목록을 모두 조회하세요.
select *
from Lot;

-- 문제 3
-- 장비 목록을 모두 조회하세요.
select *
from equipment;

-- 문제 4
-- RunHistory 전체를 조회하세요.
select *
from runhistory;

-- 문제 5
-- Sensor 전체를 조회하세요.
select *
from sensor;

-- 문제 6
-- SensorMeasurement 전체를 조회하세요.
select *
from sensormeasurement;


-- 2. 특정 상태 찾기
-- 문제 7
-- warning 상태인 Lot를 조회하세요.
select *
from Lot
where lot_status = 'warning';

-- 문제 8
-- warning 상태인 Run을 조회하세요.
select *
from runhistory
where run_status = 'warning';

-- 문제 9
-- active 상태인 장비를 조회하세요.
select *
from equipment
where status = 'active';

-- 3. 특정 대상만 보기
-- 문제 10
-- Lot L003의 Run 기록을 조회하세요.
select *
from runhistory
where lot_id = 'L003';

-- 문제 11
-- Run 7의 센서 측정값을 조회하세요.
select *
from sensormeasurement
where run_id = '7'; 

-- 문제 12
-- 장비 101에 부착된 센서를 조회하세요.
select *
from equipment e
join sensor s on e.equipment_id = s.equipment_id
where e.equipment_id = '101'



-- 4. 시간 순서로 보기
-- 문제 13
-- RunHistory를 시작 시간 순으로 조회하세요.
select *
from runhistory
order by start_time;

-- 문제 14
-- Run 7의 센서 측정값을 측정 시간 순으로 조회하세요.
select *
from sensormeasurement
where run_id = '7'
order by measured_at;

-- 5. 정상 범위 감각 잡기
-- 문제 15


-- 문제 16
-- measured_value가 410보다 큰 센서 측정값을 조회하세요.
select *
from sensormeasurement
where measured_value > 410;

-- 문제 17
-- measured_value가 2.2보다 큰 센서 측정값을 조회하세요.
select *
from sensormeasurement
where measured_value > 2.2;