create database MyRetail;

use MyRetail;

create table Products
(
    Id int not null identity(1,1) primary key,
    [Name] varchar(30) not null,
    Description varchar(5000) not null,
    Sku varchar(16) not null,
    AvailableOnline bit not null,
    constraint [Sku_Must_Be_Exactly_16_Chars_Long] check (LEN([Sku]) = 16),
    constraint Sku_Contains_Only_Characters_And_Numbers check (Sku NOT LIKE '%[^A-Z0-9]%')
);


create table Locations
(
    Id int not null identity(1,1) primary key,
    [Name] varchar(30) not null,
    AddressStreetLine1 varchar(100) not null,
    AddressCity varchar(100) not null,
    AddressState varchar(2) not null,
    AddressZip varchar(15) not null
);

create table ProductLocations
(
    ProductId int not null,
    LocationId int not null,
    CONSTRAINT Product_Location_Id PRIMARY KEY (ProductId, LocationId),
    CONSTRAINT Fk_Products FOREIGN KEY (ProductId) REFERENCES Products (Id),
    CONSTRAINT Fk_Locations FOREIGN KEY (LocationId) REFERENCES Locations (Id),
    CONSTRAINT Locations_Can_Carry_The_Same_Product_Only_Once UNIQUE (ProductId, LocationId)
);
