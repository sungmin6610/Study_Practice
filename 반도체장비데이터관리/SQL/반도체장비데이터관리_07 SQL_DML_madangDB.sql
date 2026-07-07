insert into book(bookid, bookname, publisher, price)
values (11, '스포츠 의학', '한솔의학서적', 90000),
       (12, '스포츠 의학', '한솔의학서적', 90000),
	   (13, '스포츠 의학', '한솔의학서적', 90000);

insert into book(bookid, bookname, publisher)
values (14, '스포츠 의학', '한솔의학서적');

-- =========================================
-- 마당서점 DB 데이터 조작어(DML) 실습문제
-- 테이블
-- Book(bookid, bookname, publisher, price)
-- Orders(orderid, custid, bookid, saleprice, orderdate)
-- Customer(custid, name, address, phone)
-- =========================================
select*
from book;

select*
from customer;

select*
from orders;


-- [INSERT]

-- 문제 1
-- 새로운 고객을 추가하세요.
-- 고객번호: 6
-- 이름: 김하늘
-- 주소: 서울 서초구
-- 전화번호: 010-1111-2222
insert into customer(custid, name, address, phone)
values (6, '김하늘', '서울 서초구', '010-1111-2222');

-- 문제 2
-- 새로운 도서를 추가하세요.
-- 도서번호: 16
-- 도서명: 데이터 분석 입문
-- 출판사: 한빛아카데미
-- 가격: 22000
insert into book(bookid, bookname, publisher, price)
values (16, '데이터 분석 입문', '한빛아카데미', '22000');

-- 문제 3
-- 고객 6번이 도서 11번을 21000원에 주문했습니다. 주문 정보를 추가하세요.
-- 주문번호: 11
-- 고객번호: 6
-- 도서번호: 11
-- 판매가격: 21000
-- 주문일자: 2024-04-01
insert into orders(orderid, custid, bookid, saleprice, orderdate)
values (11, '6', '11', '21000', '2024-04-01');

-- 문제 4
-- 새로운 고객을 추가하세요.
-- 고객번호: 7
-- 이름: 이수민
-- 주소: 경기 성남시
-- 전화번호: 010-3333-4444
insert into customer(custid, name, address, phone)
values (7, '이수민', '경기 성남시', '010-3333-4444');

-- 문제 5
-- 새로운 도서를 추가하세요.
-- 도서번호: 17
-- 도서명: SQL 연습문제집
-- 출판사: 이지스퍼블리싱
-- 가격: 18000
insert into book(bookid, bookname, publisher, price)
values (17, 'SQL 연습문제집', '이지스퍼블리싱', '18000');


-- [UPDATE]

-- 문제 6
-- 도서번호 16번의 가격을 23000원으로 수정하세요.
update book
set price = 23000
where bookid = 16;

-- 문제 7
-- 고객번호 7번의 주소를 경기 용인시로 수정하세요.
update customer
set address = '경기 용인시'
where custid = 7;

-- 문제 8
-- 주문번호 16번의 판매가격을 20000원으로 수정하세요.
update orders
set saleprice = 20000
where orderid = 16;


-- [DELETE]

-- 문제 9
-- 고객번호 7번 고객을 삭제하세요.
delete from customer
where custid = 7;

-- 문제 10
-- 고객번호 6번 고객을 삭제해 보세요.
-- 실행 결과를 확인하고, 왜 오류가 발생하는지 생각해 보세요.
delete from customer
where custid = 6;

-- select *
-- from orders;


