-- 반도체장비데이터관리 03강 
-- GROUP BY 1단계 연습문제
use madangDB;
-- 기본 문제
-- 1. Book 테이블에서 출판사 목록을 조회하시오.
select publisher from book group by publisher;
-- 2. Orders 테이블에서 주문이 발생한 고객 ID 목록을 조회하시오.
select custid from orders group by custid;
-- 3. Orders 테이블에서 주문된 도서 ID 목록을 조회하시오.
select bookid from orders group by bookid;

-- 조건 포함 문제

-- 1. 가격이 20,000원 이상인 도서의 출판사 목록을 조회하시오.
select publisher from book where price >= 20000 group by publisher;
-- 2. 판매 가격이 10,000원 이상인 주문의 고객 ID 목록을 조회하시오.
select custid from orders where saleprice >= 10000 group by custid;
-- 3. 2024년 이후 주문된 날짜 목록을 조회하시오.
select orderdate from orders where orderdate >= '2024-01-01' group by orderdate;
-- 4. 전화번호가 입력된 고객의 주소 목록을 조회하시오.
select address from customer where phone is not null;
-- 5. 출판사가 '굿스포츠'인 도서의 이름 목록을 조회하시오.
select bookname from book where publisher = '굿스포츠';
