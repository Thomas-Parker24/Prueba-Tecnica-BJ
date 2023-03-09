use BJ;

create table TipoPersona(

	idTipoPersona int identity (1,1) primary key,
	tipoPersona varchar (50) not null
);

create table Area(
	
	idArea int identity (1,1) primary key,
	Area varchar (50) not null

);

create table Persona (

	 documento int  primary key,
	 nombre varchar (50) not null,
	 idTipoPersona int not null,

	 foreign key (idTipoPersona) references TipoPersona (idTipoPersona)
);

create table Ingreso (

	idIngreso int identity (1,1) primary key,
	idPersona int not null,
	fechaIngreso date not null,
	horaEvento int not null,
	MinutoEvento int not null

	foreign key (idPersona) references Persona(documento)
);

create table Excusa (
	
	idExcusa int identity (1,1) primary key,
	varlorExcusa varchar (50) not null
);

create table Salida (
	
	idSalida int identity (1,1) primary key,
	idPersona int not null,
	fechaSalida date not null,
	horaEvento int not null,
	MinutoEvento int not null,
	excusa int 

	foreign key (idPersona) references Persona (documento),
	foreign key (excusa) references Excusa(idExcusa)
);

create table Empleado(
	
	documento int primary key,
	area int not null,

	foreign key (documento) references Persona(documento),
	foreign key (area) references Area (idArea)
);

insert Area (Area) values ('Sistemas');
insert Area (Area) values ('Mercadeo');
insert Area (Area) values ('Produccion');

insert Excusa (varlorExcusa) values ('Cita médica');
insert Excusa (varlorExcusa) values ('Calamidad');
insert Excusa (varlorExcusa) values ('Diligencia personal');

insert TipoPersona (tipoPersona) values ('Empleado');
insert TipoPersona (tipoPersona) values ('Proveedor');
insert TipoPersona (tipoPersona) values ('Invitado');
insert TipoPersona (tipoPersona) values ('Centro de costos');



