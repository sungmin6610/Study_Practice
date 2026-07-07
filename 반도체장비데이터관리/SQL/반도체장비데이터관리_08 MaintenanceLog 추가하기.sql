-- =========================================
-- MaintenanceLog 테이블 생성 및 데이터 입력 실습
-- =========================================
use semicon_equipDB;
-- 문제 1
-- 장비의 유지보수 이력을 저장할 MaintenanceLog 테이블을 생성하세요.
      create table maintenancelog(
      maintenance_id int primary key,
      equipment_id int not null,
      maintenance_date date not null,
      engineer_name varchar(50) not null,
      maintenance_type varchar(30) not null,
      details varchar(255),
      foreign key (equipment_id) references equipment(equipment_id)
      );
-- maintenance_
-- maintenance_id, equipment_id, maintenance_date, engineer_name, maintenance_type, details
-- 조건:
-- maintenance_id는 기본키
-- equipment_id는 Equipment 테이블의 equipment_id를 참조하는 외래키
-- maintenance_date, engineer_name, maintenance_type은 NULL을 허용하지 않음


-- 문제 2
-- 다음 유지보수 이력을 추가하세요.
-- 유지보수번호: 1
-- 장비번호: 101
-- 점검일자: 2024-03-03
-- 담당자: 최정우
-- 점검유형: 정기점검
-- 상세내용: 챔버 내부 청소 및 압력 점검
insert into maintenancelog
values (1, 101, '2024-03-03', '최정우', '정기점검', '챔버 내부 청소 및 압력 점검');

-- 문제 3
-- 다음 유지보수 이력을 추가하세요.
-- 유지보수번호: 2
-- 장비번호: 102
-- 점검일자: 2024-03-06
-- 담당자: 박수현
-- 점검유형: 수리
-- 상세내용: 연마 균일도 문제로 부품 교체
insert into maintenancelog
values (2, 102, '2024-03-06', '박수현', '수리', '연마 균일도 문제로 부품 교체');

-- 문제 4
-- 다음 유지보수 이력을 추가하세요.
-- 유지보수번호: 3
-- 장비번호: 104
-- 점검일자: 2024-03-09
-- 담당자: 이도현
-- 점검유형: 정기점검
-- 상세내용: 노후 부품 상태 확인
insert into maintenancelog
values (3, 104, '2024-03-09', '이도현', '정기점검', '노후 부품 상태 확인');

-- 문제 5
-- 다음 유지보수 이력을 추가하세요.
-- 유지보수번호: 4
-- 장비번호: 106
-- 점검일자: 2024-03-21
-- 담당자: 김서윤
-- 점검유형: 초기점검
-- 상세내용: 신규 장비 설치 후 초기 테스트
insert into maintenancelog
values (4, 106, '2024-03-21', '김서윤', '초기점검', '신규 장비 설치 후 초기 테스트');

-- 문제 6
-- 유지보수번호 4번의 점검유형을 '성능점검'으로 수정하세요.
update maintenancelog
set details = '성능점검'
where maintenance_id =4;

-- 문제 7
-- 유지보수번호 3번의 상세내용을 '노후 부품 교체 여부 검토'로 수정하세요.
update maintenancelog
set details = '노후 부품 교체 여부 검토'
where maintenance_id = 3;

-- 문제 8
-- 장비번호 999번에 대한 유지보수 이력을 추가해 보세요.
-- 실행 결과를 확인하고, 왜 오류가 발생하는지 생각해 보세요.
-- 유지보수번호: 5
-- 장비번호: 999
-- 점검일자: 2024-03-25
-- 담당자: 홍길동
-- 점검유형: 정기점검
-- 상세내용: 존재하지 않는 장비 테스트
insert into maintenancelog
values (5, 999, '2024-03-25', '홍길동', '정기점검', '존재하지 않는 장비 테스트');