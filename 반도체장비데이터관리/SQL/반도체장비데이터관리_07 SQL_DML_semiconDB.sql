-- =========================================
-- SemiconDB 데이터 조작어(DML) 실습문제
-- =========================================
use Semicon_EquipDB;


-- [INSERT]

-- 문제 1
-- 새로운 사용자를 추가하세요.
-- 사용자번호: 6
-- 이름: 김지후
-- 부서: 생산기술팀
-- 연락처: 010-6789-1111
insert into EquipmentUser(user_id, name, department, contact)
values(6, '김지후', '생산기술팀', '010-6789-1111');

-- 문제 2
-- 새로운 장비를 추가하세요.
-- 장비번호: 106
-- 모델명: CVD-C900
-- 설치일자: 2024-03-20
-- 상태: active
insert into Equipment(equipment_id, model_name, install_date, status)
values(106, 'CVD-C900', '2024-03-20', 'active');

-- 문제 3
-- 사용자 6번이 장비 106번을 사용한 기록을 추가하세요.
-- 로그번호: 12
-- 사용자번호: 6
-- 장비번호: 106
-- 사용일자: 2024-03-18
-- 문제보고: null
insert into Usagelog(log_id, user_id, equipment_id, use_date, issue_report)
values(12, 6, 106, '2024-03-18', 'null');

-- 문제 4
-- 새로운 사용자를 추가하세요.
-- 사용자번호: 7
-- 이름: 한소라
-- 부서: 품질팀
-- 연락처: 010-7890-2222
insert into EquipmentUser(user_id, name, department, contact)
values(7, '한소라', '품질팀', '010-7890-2222');

-- 문제 5
-- 새로운 장비를 추가하세요.
-- 장비번호: 107
-- 모델명: IMPLANT-Q400
-- 설치일자: 2024-03-25
-- 상태: active
insert into Equipment(equipment_id, model_name, install_date, status)
values(107, 'IMPLANT-Q400', '2024-03-25', 'active');


-- [UPDATE]

-- 문제 6
-- 장비번호 106번의 상태를 maintenance로 수정하세요.
update equipment
set status = 'maintenance'
where equipment_id = 106;

-- 문제 7
-- 사용자번호 7번의 부서를 개발팀으로 수정하세요.
update equipmentuser
set department = '개발팀'
where user_id = 7;

-- 문제 8
-- 로그번호 12번의 문제보고 내용을
-- '초기 테스트 완료'로 수정하세요.
update usagelog
set issue_report = '초기 테스트 완료'
where log_id = 12;

-- [DELETE]

-- 문제 9
-- 사용자번호 7번 사용자를 삭제하세요.
delete from equipmentuser
where user_id = 7;

-- 문제 10
-- 사용자번호 6번 사용자를 삭제해 보세요.
-- 실행 결과를 확인하고, 왜 오류가 발생하는지 생각해 보세요.
-- usagelog에 6번 사용자 데이터가 있기 때문.
delete from equipmentuser
where user_id = 6;