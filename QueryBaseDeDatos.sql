
CREATE DATABASE bdProyectos
USE bdProyectos
go
CREATE TABLE Usuario (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Contrasena NVARCHAR(255) NOT NULL,
    RutaAvatar NVARCHAR(MAX) NULL,
    Activo BIT NOT NULL DEFAULT 1
);
go
CREATE TABLE TipoResponsable (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(255) NOT NULL,
    CONSTRAINT UQ_TipoResponsable_Titulo UNIQUE (Titulo)
);
go
CREATE TABLE Responsable (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdTipoResponsable INT NOT NULL,
    IdUsuario INT NOT NULL,
    Nombre NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_Responsable_TipoResponsable FOREIGN KEY (IdTipoResponsable) REFERENCES TipoResponsable(Id),
    CONSTRAINT FK_Responsable_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id) ON DELETE CASCADE
);
go
CREATE TABLE TipoProyecto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(255) NOT NULL,
    CONSTRAINT UQ_TipoProyecto_Nombre UNIQUE (Nombre)
);
go
CREATE TABLE Estado (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(255) NOT NULL,
    CONSTRAINT UQ_Estado_Nombre UNIQUE (Nombre)
);
go
CREATE TABLE Proyecto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdProyectoPadre INT NULL,
    IdResponsable INT NOT NULL,
    IdTipoProyecto INT NOT NULL,
    Codigo NVARCHAR(50) NULL,
    Titulo NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    FechaInicio DATE NULL,
    FechaFinPrevista DATE NULL,
    FechaModificacion DATE NULL,
    FechaFinalizacion DATE NULL,
    RutaLogo NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Proyecto_ProyectoPadre FOREIGN KEY (IdProyectoPadre) REFERENCES Proyecto(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Proyecto_Responsable FOREIGN KEY (IdResponsable) REFERENCES Responsable(Id),
    CONSTRAINT FK_Proyecto_TipoProyecto FOREIGN KEY (IdTipoProyecto) REFERENCES TipoProyecto(Id)
);
go
CREATE TABLE Estado_Proyecto (
    IdProyecto INT PRIMARY KEY,
    IdEstado INT NOT NULL,
    CONSTRAINT FK_EstadoProyecto_Proyecto FOREIGN KEY (IdProyecto) REFERENCES Proyecto(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EstadoProyecto_Estado FOREIGN KEY (IdEstado) REFERENCES Estado(Id)
);
go
CREATE TABLE TipoProducto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(255) NOT NULL,
    CONSTRAINT UQ_TipoProducto_Nombre UNIQUE (Nombre)
);
go
CREATE TABLE Producto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdTipoProducto INT NOT NULL,
    Codigo NVARCHAR(50) NULL,
    Titulo NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    FechaInicio DATE NULL,
    FechaFinPrevista DATE NULL,
    FechaModificacion DATE NULL,
    FechaFinalizacion DATE NULL,
    RutaLogo NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Producto_TipoProducto FOREIGN KEY (IdTipoProducto) REFERENCES TipoProducto(Id)
);
go
CREATE TABLE Proyecto_Producto (
    IdProyecto INT NOT NULL,
    IdProducto INT NOT NULL,
    FechaAsociacion DATE NULL,
    PRIMARY KEY (IdProyecto, IdProducto),
    CONSTRAINT FK_ProyectoProducto_Proyecto FOREIGN KEY (IdProyecto) REFERENCES Proyecto(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProyectoProducto_Producto FOREIGN KEY (IdProducto) REFERENCES Producto(Id) ON DELETE CASCADE
);
go
CREATE TABLE Entregable (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(50) NULL,
    Titulo NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    FechaInicio DATE NULL,
    FechaFinPrevista DATE NULL,
    FechaModificacion DATE NULL,
    FechaFinalizacion DATE NULL
);
go
CREATE TABLE Producto_Entregable (
    IdProducto INT NOT NULL,
    IdEntregable INT NOT NULL,
    FechaAsociacion DATE NULL,
    PRIMARY KEY (IdProducto, IdEntregable),
    CONSTRAINT FK_ProductoEntregable_Producto FOREIGN KEY (IdProducto) REFERENCES Producto(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductoEntregable_Entregable FOREIGN KEY (IdEntregable) REFERENCES Entregable(Id) ON DELETE CASCADE
);
go
CREATE TABLE Responsable_Entregable (
    IdResponsable INT NOT NULL,
    IdEntregable INT NOT NULL,
    FechaAsociacion DATE NULL,
    PRIMARY KEY (IdResponsable, IdEntregable),
    CONSTRAINT FK_ResponsableEntregable_Responsable FOREIGN KEY (IdResponsable) REFERENCES Responsable(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ResponsableEntregable_Entregable FOREIGN KEY (IdEntregable) REFERENCES Entregable(Id) ON DELETE CASCADE
);
go
CREATE TABLE Archivo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    Ruta NVARCHAR(MAX) NOT NULL,
    Nombre NVARCHAR(255) NOT NULL,
    Tipo NVARCHAR(50) NULL,
    Fecha DATE NULL,
    CONSTRAINT FK_Archivo_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id) ON DELETE CASCADE
);
go
CREATE TABLE Archivo_Entregable (
    IdArchivo INT NOT NULL,
    IdEntregable INT NOT NULL,
    PRIMARY KEY (IdArchivo, IdEntregable),
    CONSTRAINT FK_ArchivoEntregable_Archivo FOREIGN KEY (IdArchivo) REFERENCES Archivo(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ArchivoEntregable_Entregable FOREIGN KEY (IdEntregable) REFERENCES Entregable(Id) ON DELETE CASCADE
);
go
CREATE TABLE Actividad (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdEntregable INT NOT NULL,
    Titulo NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    FechaInicio DATE NULL,
    FechaFinPrevista DATE NULL,
    FechaModificacion DATE NULL,
    FechaFinalizacion DATE NULL,
    Prioridad INT NULL,
    PorcentajeAvance INT CHECK (PorcentajeAvance BETWEEN 0 AND 100),
    CONSTRAINT FK_Actividad_Entregable FOREIGN KEY (IdEntregable) REFERENCES Entregable(Id) ON DELETE CASCADE
);
go
CREATE TABLE Presupuesto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdProyecto INT NOT NULL,
    MontoSolicitado DECIMAL(15,2) NOT NULL,
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Pendiente' CHECK (Estado IN ('Pendiente','Aprobado','Rechazado')),
    MontoAprobado DECIMAL(15,2) NULL,
    PeriodoAnio INT NULL,
    FechaSolicitud DATE NULL,
    FechaAprobacion DATE NULL,
    Observaciones NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Presupuesto_Proyecto FOREIGN KEY (IdProyecto) REFERENCES Proyecto(Id) ON DELETE CASCADE
);
go
CREATE TABLE DistribucionPresupuesto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdPresupuestoPadre INT NOT NULL,
    IdProyectoHijo INT NOT NULL,
    MontoAsignado DECIMAL(15,2) NOT NULL,
    CONSTRAINT FK_Distribucion_Presupuesto FOREIGN KEY (IdPresupuestoPadre) REFERENCES Presupuesto(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Distribucion_Proyecto FOREIGN KEY (IdProyectoHijo) REFERENCES Proyecto(Id) ON DELETE NO ACTION
);
go
CREATE TABLE EjecucionPresupuesto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdPresupuesto INT NOT NULL,
    Anio INT NOT NULL,
    MontoPlaneado DECIMAL(15,2) NULL,
    MontoEjecutado DECIMAL(15,2) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Ejecucion_Presupuesto FOREIGN KEY (IdPresupuesto) REFERENCES Presupuesto(Id) ON DELETE CASCADE
);
go
CREATE TABLE VariableEstrategica (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL
);
go
CREATE TABLE ObjetivoEstrategico (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdVariable INT NOT NULL,
    Titulo NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    CONSTRAINT FK_ObjetivoEstrategico_Variable FOREIGN KEY (IdVariable) REFERENCES VariableEstrategica(Id) ON DELETE CASCADE
);
go
CREATE TABLE MetaEstrategica (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdObjetivo INT NOT NULL,
    Titulo NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    CONSTRAINT FK_MetaEstrategica_Objetivo FOREIGN KEY (IdObjetivo) REFERENCES ObjetivoEstrategico(Id) ON DELETE CASCADE
);
go
CREATE TABLE Meta_Proyecto (
    IdMeta INT NOT NULL,
    IdProyecto INT NOT NULL,
    FechaAsociacion DATE NULL,
    PRIMARY KEY (IdMeta, IdProyecto),
    CONSTRAINT FK_MetaProyecto_Meta FOREIGN KEY (IdMeta) REFERENCES MetaEstrategica(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MetaProyecto_Proyecto FOREIGN KEY (IdProyecto) REFERENCES Proyecto(Id) ON DELETE CASCADE
);
go