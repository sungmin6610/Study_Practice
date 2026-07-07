-- =========================================
-- 🧪 SQL DDL 제약조건 실습 시나리오
-- =========================================

-- -----------------------------------------
-- 0. 기존 테이블 삭제 (실습 반복용)
-- -----------------------------------------
DROP TABLE IF EXISTS NewOrders;
DROP TABLE IF EXISTS NewBook;
DROP TABLE IF EXISTS NewCustomer;


-- -----------------------------------------
-- 1. 테이블 생성
-- -----------------------------------------

CREATE TABLE NewCustomer (
    custid   INTEGER PRIMARY KEY,
    name     VARCHAR(40),
    address  VARCHAR(40),
    phone    VARCHAR(30)
);

CREATE TABLE NewBook (
    bookname   VARCHAR(20) NOT NULL,
    publisher  VARCHAR(20) UNIQUE,
    price      INTEGER DEFAULT 10000 CHECK(price >= 1000),
    PRIMARY KEY (bookname, publisher)
);

CREATE TABLE NewOrders (
    orderid    INTEGER,
    custid     INTEGER NOT NULL,
    bookid     INTEGER,
    saleprice  INTEGER,
    orderdate  DATE,
    PRIMARY KEY(orderid),
    FOREIGN KEY(custid)
        REFERENCES NewCustomer(custid)
        ON DELETE CASCADE
);


-- -----------------------------------------
-- 2. 고객 데이터 입력
-- -----------------------------------------

-- [정상]
INSERT INTO NewCustomer VALUES (1, 'Alice', 'Seoul', '010-1111-1111');
INSERT INTO NewCustomer VALUES (2, 'Bob', 'Busan', '010-2222-2222');


-- -----------------------------------------
-- 3. 도서 데이터 입력 (제약조건 테스트)
-- -----------------------------------------

-- [정상]
INSERT INTO NewBook VALUES ('SQL입문', '한빛출판사', 15000);

-- [실패 예상] NOT NULL 위반
-- INSERT INTO NewBook VALUES (NULL, '길벗', 12000);

-- [실패 예상] UNIQUE 위반
-- INSERT INTO NewBook VALUES ('데이터베이스', '한빛출판사', 20000);

-- [정상] DEFAULT 적용
INSERT INTO NewBook (bookname, publisher)
VALUES ('AI개론', '위키북스');

-- [실패 예상] CHECK 위반
-- INSERT INTO NewBook VALUES ('파이썬', '이지스퍼블리싱', 500);


-- -----------------------------------------
-- 4. 주문 데이터 입력 (외래키 테스트)
-- -----------------------------------------

-- [정상]
INSERT INTO NewOrders VALUES (1, 1, 101, 12000, '2024-03-01');

-- [실패 예상] 존재하지 않는 고객
-- INSERT INTO NewOrders VALUES (2, 999, 101, 12000, '2024-03-02');

-- [문제 상황] 존재하지 않는 bookid인데도 성공
INSERT INTO NewOrders VALUES (3, 1, 9999, 13000, '2024-03-03');


-- -----------------------------------------
-- ⚠️ 문제 인식
-- -----------------------------------------
-- bookid에 외래키가 없어서
-- 존재하지 않는 값도 저장됨


-- -----------------------------------------
-- 5. 구조 개선 (bookid 외래키 추가 준비)
-- -----------------------------------------

-- 5-1. NewBook에 bookid 컬럼 추가
ALTER TABLE NewBook ADD bookid INTEGER;

-- 5-2. 기존 데이터에 bookid 부여
UPDATE NewBook SET bookid = 1 WHERE bookname = 'SQL입문';
UPDATE NewBook SET bookid = 2 WHERE bookname = 'AI개론';


-- -----------------------------------------
-- 6. 기본키 변경 (DBMS별 차이 있음)
-- -----------------------------------------
-- ※ 실제 DBMS에 따라 constraint 이름 확인 필요

-- 예시 (PostgreSQL 기준)
-- ALTER TABLE NewBook DROP CONSTRAINT NewBook_pkey;

-- MySQL은 DROP PRIMARY KEY 사용
ALTER TABLE NewBook DROP PRIMARY KEY;

ALTER TABLE NewBook ADD PRIMARY KEY (bookid);


-- -----------------------------------------
-- 7. 외래키 추가
-- -----------------------------------------

ALTER TABLE NewOrders
ADD CONSTRAINT fk_book
FOREIGN KEY (bookid)
REFERENCES NewBook(bookid);


-- -----------------------------------------
-- 8. 외래키 적용 후 테스트
-- -----------------------------------------

-- [실패 예상] 존재하지 않는 bookid
-- INSERT INTO NewOrders VALUES (4, 1, 9999, 14000, '2024-03-04');

-- [정상]
INSERT INTO NewOrders VALUES (5, 1, 1, 14000, '2024-03-05');


-- -----------------------------------------
-- 9. 결과 확인
-- -----------------------------------------

SELECT * FROM NewCustomer;
SELECT * FROM NewBook;
SELECT * FROM NewOrders;