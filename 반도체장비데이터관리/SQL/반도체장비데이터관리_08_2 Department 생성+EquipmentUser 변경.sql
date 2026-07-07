-- =========================================
-- Department 생성 + EquipmentUser 구조 변경
-- =========================================
use semicon_equipdb;

-- 1. Department 테이블 생성
create table department(
dept_id int primary key,
dept_name varchar(50) not null unique,
magager_name varchar(50),
office_location varchar(50)
);

alter table department
change magager_name manager_name varchar(50);

select *
from department;

-- 2. Department 데이터 입력
insert into department values (1, '제조팀', '김부장', 'A동 2층');
insert into department values (2, '품질팀', '이과장', 'B동 1층');
insert into department values (3, '연구팀', '박차장', 'A동 1층');
insert into department values (4, '개발팀', '최팀장', '연구동 3층');
insert into department values (5, '생산기술팀', '송팀장', '연구동 3층');

-- 3. EquipmentUser에 dept_id 컬럼 추가
alter table equipmentuser add dept_id int;


-- 4. 기존 department 값을 기반으로 EquipmentUser의 dept_id 채우기
update equipmentuser eu
join department d
on eu.department = d.dept_name
set eu.dept_id = d.dept_id;

-- 5. EquipmentUser의 dept_id를 NOT NULL로 변경
alter table equipmentuser
modify dept_id int not null;


-- 6. EquipmentUser에 외래키 추가
alter table equipmentuser
add constraint fk_employee_department
foreign key (dept_id)
references department(dept_id);


-- 7. EquipmentUser의 기존 department 컬럼 삭제
alter table equipmentuser
drop column department;


-- 8. 최종 구조 확인
SELECT * FROM Department;

SELECT * FROM EquipmentUser;
DESC EquipmentUser;