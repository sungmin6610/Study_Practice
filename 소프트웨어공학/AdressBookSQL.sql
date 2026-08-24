use library;
create table contacts(
id int auto_increment primary key,
name varchar(20) not null,
phone varchar(20) not null,
email varchar(50),
address varchar(50)
);

desc contacts;

insert into contacts values(null, '홍길동', '010-1234-1234', 'hong@naver.com', '경기도 안성시 공도읍');
insert into contacts values(null, '김하늘', '010-1111-2222', 'sky@naver.com', '경기도 수원시 장안동');
select * from contacts;