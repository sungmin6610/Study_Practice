-- =========================================
-- JOIN 연습문제
-- ① 힌트형 문제 10개
-- ② 요구사항 해석형 문제 10개
-- =========================================

-- =========================================
-- 문제를 풀기 전에 생각하기
-- 1. 필요한 정보:
-- 2. 그 정보가 있는 테이블:
-- =========================================

-- =========================================
-- ① 힌트형 문제
-- 사용할 테이블 힌트가 함께 제시됩니다.
-- =========================================

-- 1. 최근 주문 내역을 빠르게 확인하기 위해,
-- 주문번호, 고객이름, 주문일자를 조회하시오.
-- (Orders, Customer)
select orderid, orderdate, name
from orders
join customer
on orders.custid = customer.custid;

-- 2. 어떤 책이 실제로 얼마에 판매되었는지 비교하려고 한다.
-- 도서명, 정가(price), 판매가격(saleprice)을 조회하시오.
-- (Orders, Book)
select saleprice, bookname, price
from orders
join book
on orders.saleprice = book.price;

-- 3. 특정 고객의 주문 이력을 확인하려고 한다.
-- 고객번호가 2인 고객의 이름, 주문번호, 주문일자를 조회하시오.
-- (Orders, Customer)
select name, orderid, orderdate
from orders
join customer
on orders.custid = customer.custid
where customer.custid = 2;

-- 4. 한 출판사의 책이 실제로 주문되었는지 확인하려고 한다.
-- 출판사가 '한빛미디어'인 도서의 주문번호와 도서명을 조회하시오.
-- (Orders, Book)
select orderid, bookname
from orders
join book
on orders.bookid = book.bookid
where book.publisher = '대한미디어';

-- 5. 고객 응대 중 본인 확인을 위해 주문 기록과 연락처를 함께 보려고 한다.
-- 고객이름, 전화번호, 주문번호를 조회하시오.
-- (Orders, Customer)
select name, phone, orderid
from orders
join customer
on orders. custid = customer.custid;

-- 6. 도서 판매 현황을 가격 기준으로 살펴보기 위해,
-- 도서명과 판매가격을 조회하되, 판매가격이 높은 순으로 정렬하시오.
-- (Orders, Book)
select bookname, saleprice
from orders
join book
on orders.bookid = book.bookid
order by saleprice desc;

-- 7. 서울 지역 고객의 주문 현황만 따로 확인하려고 한다.
-- 주소가 '대한민국 서울'인 고객의 이름과 주문일자를 조회하시오.
-- (Orders, Customer)
select address, name, orderdate
from orders
join customer
on orders.custid = customer.custid
where customer.address = '대한민국 서울';

-- 8. 정가보다 할인되어 판매된 도서를 찾으려고 한다.
-- 도서명, 정가(price), 판매가격(saleprice)을 조회하시오.
-- 단, 정가보다 낮은 가격에 판매된 경우만 조회하시오.
-- (Orders, Book)
select bookname, price, saleprice
from orders
join book
on orders.bookid = book.bookid
where orders.saleprice < book.price;

-- 9. 주문 처리 순서를 보기 위해 최근 주문부터 확인하려고 한다.
-- 고객이름과 주문번호를 조회하되, 주문번호 기준 내림차순으로 정렬하시오.
-- (Orders, Customer)
select name, orderid
from orders
join customer
on orders.custid = customer.custid
order by orderid desc;

-- 10. 고가 도서의 실제 판매 내역을 확인하려고 한다.
-- 정가가 20000원 이상인 도서의 이름과 판매가격을 조회하시오.
-- (Orders, Book)
select bookname, saleprice
from orders
join book
on orders.bookid = book.bookid
where book.price >= 20000;


-- =========================================
-- ② 요구사항 해석형 문제
-- 테이블 힌트 없이, 요구를 SQL로 해석해보세요.
-- =========================================

-- 11. “누가 언제 주문했는지 한눈에 보고 싶어요.”
-- 주문 기록마다 주문한 사람의 이름과 주문일자가 보이도록 조회하시오.


-- 12. “책 원래 가격이랑 실제로 팔린 가격을 같이 보고 싶어요.”
-- 주문된 도서에 대해 도서명, 정가, 판매가격을 조회하시오.


-- 13. “서울 고객들 주문만 따로 볼 수 있을까요?”
-- 서울에 거주하는 고객의 이름과 주문번호를 조회하시오.


-- 14. “대한미디어 책이 실제로 얼마나 나갔는지 보고 싶어요.”
-- 출판사가 '대한미디어'인 도서의 주문번호와 도서명을 조회하시오.


-- 15. “고객 전화 받았을 때 바로 주문기록도 같이 보고 싶어요.”
-- 고객이름, 전화번호, 주문번호를 조회하시오.


-- 16. “비싸게 팔린 주문부터 먼저 보고 싶어요.”
-- 주문된 도서에 대해 도서명과 판매가격을 조회하되, 판매가격이 높은 순으로 정렬하시오.


-- 17. “할인돼서 팔린 책들만 확인하고 싶어요.”
-- 정가보다 낮은 가격에 판매된 도서의 이름, 정가, 판매가격을 조회하시오.


-- 18. “2번 고객이 주문한 기록만 따로 보고 싶어요.”
-- 고객번호가 2인 고객의 이름, 주문번호, 주문일자를 조회하시오.


-- 19. “고가 책들은 실제로 얼마에 팔렸는지 보고 싶습니다.”
-- 정가가 20000원 이상인 도서의 이름과 판매가격을 조회하시오.


-- 20. “주문 최근 것부터 보이게 해주세요.”
-- 고객이름과 주문번호를 조회하되, 주문번호가 큰 것부터 먼저 나오도록 조회하시오.