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
CREATE OR ALTER PROCEDURE crear_responsable
    @IdTipoResponsable INT,
    @IdUsuario INT,
    @Nombre NVARCHAR(255)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM TipoResponsable WHERE Id = @IdTipoResponsable)
    BEGIN
        RAISERROR('TipoResponsable no existe', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Id = @IdUsuario)
    BEGIN
        RAISERROR('Usuario no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Responsable (IdTipoResponsable, IdUsuario, Nombre)
    VALUES (@IdTipoResponsable, @IdUsuario, @Nombre);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE actualizar_responsable
    @Id INT,
    @IdTipoResponsable INT,
    @IdUsuario INT,
    @Nombre NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validar que el responsable exista
        IF NOT EXISTS (SELECT 1 FROM Responsable WHERE Id = @Id)
        BEGIN
            RAISERROR('El responsable indicado no existe.', 16, 1);
            RETURN;
        END

        -- Validar FK: TipoResponsable
        IF NOT EXISTS (SELECT 1 FROM TipoResponsable WHERE Id = @IdTipoResponsable)
        BEGIN
            RAISERROR('El tipo de responsable no existe.', 16, 1);
            RETURN;
        END

        -- Validar FK: Usuario
        IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Id = @IdUsuario)
        BEGIN
            RAISERROR('El usuario no existe.', 16, 1);
            RETURN;
        END

        -- Actualizar registro
        UPDATE Responsable
        SET 
            IdTipoResponsable = @IdTipoResponsable,
            IdUsuario = @IdUsuario,
            Nombre = @Nombre
        WHERE Id = @Id;

        SELECT @Id AS IdActualizado, 'Responsable actualizado correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMsg NVARCHAR(MAX) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
    END CATCH
END;
GO


CREATE OR ALTER PROCEDURE crear_proyecto
    @IdResponsable INT,
    @IdTipoProyecto INT,
    @Codigo NVARCHAR(50) = NULL,
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX) = NULL,
    @FechaInicio DATE = NULL,
    @FechaFinPrevista DATE = NULL,
    @RutaLogo NVARCHAR(MAX) = NULL,
    @IdProyectoPadre INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Responsable WHERE Id = @IdResponsable)
    BEGIN
        RAISERROR('Responsable no existe', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM TipoProyecto WHERE Id = @IdTipoProyecto)
    BEGIN
        RAISERROR('TipoProyecto no existe', 16, 1);
        RETURN;
    END

    IF @IdProyectoPadre IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Proyecto WHERE Id = @IdProyectoPadre)
    BEGIN
        RAISERROR('Proyecto padre no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Proyecto (IdProyectoPadre, IdResponsable, IdTipoProyecto, Codigo, Titulo, Descripcion, FechaInicio, FechaFinPrevista, RutaLogo)
    VALUES (@IdProyectoPadre, @IdResponsable, @IdTipoProyecto, @Codigo, @Titulo, @Descripcion, @FechaInicio, @FechaFinPrevista, @RutaLogo);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO
CREATE OR ALTER PROCEDURE crear_estado_proyecto
    @IdProyecto INT,
    @IdEstado INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Proyecto WHERE Id = @IdProyecto)
    BEGIN
        RAISERROR('Proyecto no existe', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Estado WHERE Id = @IdEstado)
    BEGIN
        RAISERROR('Estado no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Estado_Proyecto (IdProyecto, IdEstado)
    VALUES (@IdProyecto, @IdEstado);
END;
GO
CREATE OR ALTER PROCEDURE crear_producto
    @p_IdTipoProducto INT,
    @p_Codigo NVARCHAR(50) = NULL,
    @p_Titulo NVARCHAR(255),
    @p_Descripcion NVARCHAR(MAX) = NULL,
    @p_FechaInicio DATE = NULL,
    @p_FechaFinPrevista DATE = NULL,
    @p_FechaModificacion DATE = NULL,
	@p_FechaFinalizacion DATE = NULL,
    @p_RutaLogo NVARCHAR(MAX) = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM TipoProducto WHERE Id = @p_IdTipoProducto)
    BEGIN
        RAISERROR('TipoProducto no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Producto (IdTipoProducto, Codigo, Titulo, Descripcion, FechaInicio, FechaFinPrevista, FechaModificacion, FechaFinalizacion, RutaLogo)
    VALUES (@p_IdTipoProducto, @p_Codigo, @p_Titulo, @p_Descripcion, @p_FechaInicio, @p_FechaFinPrevista, @p_FechaModificacion, @p_FechaFinalizacion, @p_RutaLogo);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO
CREATE OR ALTER PROCEDURE crear_proyecto_producto
    @IdProyecto INT,
    @IdProducto INT,
    @FechaAsociacion DATE = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Proyecto WHERE Id = @IdProyecto)
    BEGIN
        RAISERROR('Proyecto no existe', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Producto WHERE Id = @IdProducto)
    BEGIN
        RAISERROR('Producto no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Proyecto_Producto (IdProyecto, IdProducto, FechaAsociacion)
    VALUES (@IdProyecto, @IdProducto, @FechaAsociacion);
END;
GO
CREATE OR ALTER PROCEDURE crear_producto_entregable
    @p_IdProducto INT,
    @p_IdEntregable INT,
    @p_FechaAsociacion DATE = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Producto WHERE Id = @p_IdProducto)
    BEGIN
        RAISERROR('Producto no existe', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Entregable WHERE Id = @p_IdEntregable)
    BEGIN
        RAISERROR('Entregable no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Producto_Entregable (IdProducto, IdEntregable, FechaAsociacion)
    VALUES (@p_IdProducto, @p_IdEntregable, @p_FechaAsociacion);
END;
GO
CREATE OR ALTER PROCEDURE eliminar_producto_entregable
    @p_IdProducto INT,
    @p_IdEntregable INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Producto_Entregable WHERE IdProducto = @p_IdProducto and IdEntregable = @p_IdEntregable)
    BEGIN
        RAISERROR('Registro no existe', 16, 1);
        RETURN;
    END

    DELETE FROM Producto_Entregable WHERE IdProducto = @p_IdProducto and IdEntregable = @p_IdEntregable;
END;
GO
CREATE OR ALTER PROCEDURE actualizar_producto_entregable
    @p_IdProducto INT,
    @p_IdEntregable INT,
    @p_FechaAsociacion DATE = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Producto_Entregable WHERE IdProducto = @p_IdProducto and IdEntregable = @p_IdEntregable)
    BEGIN
        RAISERROR('Registro no existe', 16, 1);
        RETURN;
    END

    UPDATE Producto_Entregable set FechaAsociacion = @p_FechaAsociacion
	WHERE IdProducto = @p_IdProducto and IdEntregable = @p_IdEntregable;
END;
GO
CREATE OR ALTER PROCEDURE crear_responsable_entregable
    @IdResponsable INT,
    @IdEntregable INT,
    @FechaAsociacion DATE = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Responsable WHERE Id = @IdResponsable)
    BEGIN
        RAISERROR('Responsable no existe', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Entregable WHERE Id = @IdEntregable)
    BEGIN
        RAISERROR('Entregable no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Responsable_Entregable (IdResponsable, IdEntregable, FechaAsociacion)
    VALUES (@IdResponsable, @IdEntregable, @FechaAsociacion);
END;
GO
CREATE OR ALTER PROCEDURE crear_archivo
    @IdUsuario INT,
    @Ruta NVARCHAR(MAX),
    @Nombre NVARCHAR(255),
    @Tipo NVARCHAR(50) = NULL,
    @Fecha DATE = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Id = @IdUsuario)
    BEGIN
        RAISERROR('Usuario no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Archivo (IdUsuario, Ruta, Nombre, Tipo, Fecha)
    VALUES (@IdUsuario, @Ruta, @Nombre, @Tipo, @Fecha);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO
CREATE OR ALTER PROCEDURE crear_archivo_entregable
    @IdArchivo INT,
    @IdEntregable INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Archivo WHERE Id = @IdArchivo)
    BEGIN
        RAISERROR('Archivo no existe', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Entregable WHERE Id = @IdEntregable)
    BEGIN
        RAISERROR('Entregable no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Archivo_Entregable (IdArchivo, IdEntregable)
    VALUES (@IdArchivo, @IdEntregable);
END;
GO
CREATE OR ALTER PROCEDURE crear_actividad
    @p_identregable INT,
    @p_titulo NVARCHAR(255),
    @p_descripcion NVARCHAR(MAX) = NULL,
    @p_fechainicio DATE = NULL,
    @p_fechafinprevista DATE = NULL,
    @p_fechafinalizacion DATE = NULL,
    @p_prioridad INT = NULL,
    @p_porcentajeavance INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Entregable WHERE Id = @p_identregable)
    BEGIN
        RAISERROR('Entregable no existe', 16, 1);
        RETURN;
    END

    INSERT INTO Actividad
        (IdEntregable, Titulo, Descripcion, FechaInicio, FechaFinPrevista, FechaFinalizacion, Prioridad, PorcentajeAvance)
    VALUES
        (@p_identregable, @p_titulo, @p_descripcion, @p_fechainicio, @p_fechafinprevista, @p_fechafinalizacion, @p_prioridad, @p_porcentajeavance);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE crear_usuario
    @Email NVARCHAR(150),
    @Contrasena NVARCHAR(255),
    @RutaAvatar NVARCHAR(MAX) = NULL,
    @Activo BIT = 1
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Usuario WHERE Email = @Email)
    BEGIN
        RAISERROR('Ya existe un usuario con este correo electr�nico.', 16, 1);
        RETURN;
    END

    INSERT INTO Usuario (Email, Contrasena, RutaAvatar, Activo)
    VALUES (@Email, @Contrasena, @RutaAvatar, @Activo);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE actualizar_usuario
    @Id INT,
    @Email NVARCHAR(150),
    @RutaAvatar NVARCHAR(MAX) = NULL,
    @Activo BIT = 1
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Id = @Id)
    BEGIN
        RAISERROR('El usuario indicado no existe.', 16, 1);
        RETURN;
    END

    UPDATE Usuario
    SET Email = @Email, RutaAvatar = @RutaAvatar, Activo = @Activo
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE eliminar_usuario
    @Id INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Id = @Id)
    BEGIN
        RAISERROR('Usuario no encontrado.', 16, 1);
        RETURN;
    END

    DELETE FROM Usuario WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE crear_tiporesponsable
    @Titulo NVARCHAR(50),
    @Descripcion NVARCHAR(255)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM TipoResponsable WHERE Titulo = @Titulo)
    BEGIN
        RAISERROR('Ya existe un tipo de responsable con este t�tulo.', 16, 1);
        RETURN;
    END

    INSERT INTO TipoResponsable (Titulo, Descripcion)
    VALUES (@Titulo, @Descripcion);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE actualizar_tiporesponsable
    @Id INT,
    @Titulo NVARCHAR(50),
    @Descripcion NVARCHAR(255)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM TipoResponsable WHERE Id = @Id)
    BEGIN
        RAISERROR('Tipo de responsable no existe.', 16, 1);
        RETURN;
    END

    UPDATE TipoResponsable
    SET Titulo = @Titulo, Descripcion = @Descripcion
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE eliminar_tiporesponsable
    @Id INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM TipoResponsable WHERE Id = @Id)
    BEGIN
        RAISERROR('Tipo de responsable no encontrado.', 16, 1);
        RETURN;
    END

    DELETE FROM TipoResponsable WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE crear_estado
    @Nombre NVARCHAR(50),
    @Descripcion NVARCHAR(255)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Estado WHERE Nombre = @Nombre)
    BEGIN
        RAISERROR('Ya existe un estado con este nombre.', 16, 1);
        RETURN;
    END

    INSERT INTO Estado (Nombre, Descripcion)
    VALUES (@Nombre, @Descripcion);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE crear_tipoproyecto
    @Nombre NVARCHAR(150),
    @Descripcion NVARCHAR(255)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM TipoProyecto WHERE Nombre = @Nombre)
    BEGIN
        RAISERROR('Ya existe un tipo de proyecto con este nombre.', 16, 1);
        RETURN;
    END

    INSERT INTO TipoProyecto (Nombre, Descripcion)
    VALUES (@Nombre, @Descripcion);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE crear_variable
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO VariableEstrategica (Titulo, Descripcion)
    VALUES (@Titulo, @Descripcion);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE crear_objetivo
    @IdVariable INT,
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM VariableEstrategica WHERE Id = @IdVariable)
    BEGIN
        RAISERROR('Variable no existe.', 16, 1);
        RETURN;
    END

    INSERT INTO ObjetivoEstrategico (IdVariable, Titulo, Descripcion)
    VALUES (@IdVariable, @Titulo, @Descripcion);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE crear_meta
    @IdObjetivo INT,
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ObjetivoEstrategico WHERE Id = @IdObjetivo)
    BEGIN
        RAISERROR('Objetivo no existe.', 16, 1);
        RETURN;
    END

    INSERT INTO MetaEstrategica (IdObjetivo, Titulo, Descripcion)
    VALUES (@IdObjetivo, @Titulo, @Descripcion);

    SELECT SCOPE_IDENTITY() AS NuevoId;
END;
GO

CREATE OR ALTER PROCEDURE crear_meta_proyecto
    @IdMeta INT,
    @IdProyecto INT,
    @FechaAsociacion DATE = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM MetaEstrategica WHERE Id = @IdMeta)
    BEGIN
        RAISERROR('Meta no existe.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM Proyecto WHERE Id = @IdProyecto)
    BEGIN
        RAISERROR('Proyecto no existe.', 16, 1);
        RETURN;
    END

    INSERT INTO Meta_Proyecto (IdMeta, IdProyecto, FechaAsociacion)
    VALUES (@IdMeta, @IdProyecto, @FechaAsociacion);
END;
GO
CREATE OR ALTER PROCEDURE crear_entregable_con_actividades_y_archivos
    @Codigo NVARCHAR(50) = NULL,
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX) = NULL,
    @FechaInicio DATE = NULL,
    @FechaFinPrevista DATE = NULL,
    @FechaModificacion DATE = NULL,
    @FechaFinalizacion DATE = NULL,
    @Actividades NVARCHAR(MAX) = NULL,
    @Archivos NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Entregable (Codigo, Titulo, Descripcion, FechaInicio, FechaFinPrevista, FechaModificacion, FechaFinalizacion)
    VALUES (@Codigo, @Titulo, @Descripcion, @FechaInicio, @FechaFinPrevista, @FechaModificacion, @FechaFinalizacion);

    DECLARE @NuevoId INT = SCOPE_IDENTITY();
    IF @Actividades IS NOT NULL
    BEGIN
        INSERT INTO Actividad (
            IdEntregable, Titulo, Descripcion, FechaInicio,
            FechaFinPrevista, FechaFinalizacion, Prioridad, PorcentajeAvance
        )
        SELECT 
            @NuevoId,
            JSON_VALUE(a.value, '$.Titulo'),
            JSON_VALUE(a.value, '$.Descripcion'),
            TRY_CONVERT(DATE, JSON_VALUE(a.value, '$.FechaInicio')),
            TRY_CONVERT(DATE, JSON_VALUE(a.value, '$.FechaFinPrevista')),
            TRY_CONVERT(DATE, JSON_VALUE(a.value, '$.FechaFinalizacion')),
            TRY_CONVERT(INT, JSON_VALUE(a.value, '$.Prioridad')),
            TRY_CONVERT(INT, JSON_VALUE(a.value, '$.PorcentajeAvance'))
        FROM OPENJSON(@Actividades) AS a;
    END
    IF @Archivos IS NOT NULL
    BEGIN
        INSERT INTO Archivo_Entregable (IdArchivo, IdEntregable)
        SELECT 
            TRY_CONVERT(INT, JSON_VALUE(a.value, '$.IdArchivo')),
            @NuevoId
        FROM OPENJSON(@Archivos) a;
    END
    SELECT @NuevoId AS IdEntregable;
END;
GO
CREATE OR ALTER PROCEDURE crear_proyecto_con_productos
    @IdResponsable INT,
    @IdTipoProyecto INT,
    @Codigo NVARCHAR(50) = NULL,
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX) = NULL,
    @FechaInicio DATE = NULL,
    @FechaFinPrevista DATE = NULL,
    @RutaLogo NVARCHAR(MAX) = NULL,
    @IdProyectoPadre INT = NULL,
    @Productos NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Proyecto (IdProyectoPadre, IdResponsable, IdTipoProyecto, Codigo, Titulo, Descripcion, FechaInicio, FechaFinPrevista, RutaLogo)
    VALUES (@IdProyectoPadre, @IdResponsable, @IdTipoProyecto, @Codigo, @Titulo, @Descripcion, @FechaInicio, @FechaFinPrevista, @RutaLogo);

    DECLARE @NuevoId INT = SCOPE_IDENTITY();
    IF @Productos IS NOT NULL
    BEGIN
        INSERT INTO Proyecto_Producto (IdProyecto, IdProducto, FechaAsociacion)
        SELECT 
            @NuevoId,
            TRY_CONVERT(INT, JSON_VALUE(p.value, '$.IdProducto')),
            TRY_CONVERT(DATE, JSON_VALUE(p.value, '$.FechaAsociacion'))
        FROM OPENJSON(@Productos) AS p;
    END

    SELECT @NuevoId AS IdProyecto;
END;
GO
CREATE OR ALTER PROCEDURE crear_presupuesto_completo
    @IdProyecto INT,
    @MontoSolicitado DECIMAL(15,2),
    @Estado NVARCHAR(20) = 'Pendiente',
    @MontoAprobado DECIMAL(15,2) = NULL,
    @PeriodoAnio INT = NULL,
    @FechaSolicitud DATE = NULL,
    @FechaAprobacion DATE = NULL,
    @Observaciones NVARCHAR(MAX) = NULL,
    @Distribuciones NVARCHAR(MAX) = NULL,
    @Ejecuciones NVARCHAR(MAX) = NULL      
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Presupuesto (IdProyecto, MontoSolicitado, Estado, MontoAprobado, PeriodoAnio, FechaSolicitud, FechaAprobacion, Observaciones)
    VALUES (@IdProyecto, @MontoSolicitado, @Estado, @MontoAprobado, @PeriodoAnio, @FechaSolicitud, @FechaAprobacion, @Observaciones);

    DECLARE @NuevoId INT = SCOPE_IDENTITY();
    IF @Distribuciones IS NOT NULL
    BEGIN
        INSERT INTO DistribucionPresupuesto (IdPresupuestoPadre, IdProyectoHijo, MontoAsignado)
        SELECT 
            @NuevoId,
            TRY_CONVERT(INT, JSON_VALUE(d.value, '$.IdProyectoHijo')),
            TRY_CONVERT(DECIMAL(15,2), JSON_VALUE(d.value, '$.MontoAsignado'))
        FROM OPENJSON(@Distribuciones) AS d;
    END
    IF @Ejecuciones IS NOT NULL
    BEGIN
        INSERT INTO EjecucionPresupuesto (IdPresupuesto, Anio, MontoPlaneado, MontoEjecutado, Observaciones)
        SELECT 
            @NuevoId,
            TRY_CONVERT(INT, JSON_VALUE(e.value, '$.Anio')),
            TRY_CONVERT(DECIMAL(15,2), JSON_VALUE(e.value, '$.MontoPlaneado')),
            TRY_CONVERT(DECIMAL(15,2), JSON_VALUE(e.value, '$.MontoEjecutado')),
            JSON_VALUE(e.value, '$.Observaciones')
        FROM OPENJSON(@Ejecuciones) AS e;
    END

    SELECT @NuevoId AS IdPresupuesto;
END;
GO
CREATE OR ALTER PROCEDURE crear_variable_con_objetivos
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX) = NULL,
    @Objetivos NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO VariableEstrategica (Titulo, Descripcion)
    VALUES (@Titulo, @Descripcion);

    DECLARE @NuevoId INT = SCOPE_IDENTITY();

    IF @Objetivos IS NOT NULL
    BEGIN
        INSERT INTO ObjetivoEstrategico (IdVariable, Titulo, Descripcion)
        SELECT 
            @NuevoId,
            JSON_VALUE(o.value, '$.Titulo'),
            JSON_VALUE(o.value, '$.Descripcion')
        FROM OPENJSON(@Objetivos) AS o;
    END

    SELECT @NuevoId AS IdVariable;
END;
GO
CREATE OR ALTER PROCEDURE actualizar_proyecto_con_productos
    @IdProyecto INT,
    @Productos NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Proyecto_Producto WHERE IdProyecto = @IdProyecto;
    IF @Productos IS NOT NULL
    BEGIN
        INSERT INTO Proyecto_Producto (IdProyecto, IdProducto, FechaAsociacion)
        SELECT 
            @IdProyecto,
            TRY_CONVERT(INT, JSON_VALUE(p.value, '$.IdProducto')),
            TRY_CONVERT(DATE, JSON_VALUE(p.value, '$.FechaAsociacion'))
        FROM OPENJSON(@Productos) AS p;
    END;
END;
GO
CREATE OR ALTER PROCEDURE eliminar_proyecto_completo
    @IdProyecto INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Proyecto WHERE Id = @IdProyecto;
END;
GO
CREATE OR ALTER PROCEDURE actualizar_producto_con_entregables
    @IdProducto INT,
    @Entregables NVARCHAR(MAX) = NULL  
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Producto_Entregable WHERE IdProducto = @IdProducto;

    IF @Entregables IS NOT NULL
    BEGIN
        INSERT INTO Producto_Entregable (IdProducto, IdEntregable, FechaAsociacion)
        SELECT 
            @IdProducto,
            TRY_CONVERT(INT, JSON_VALUE(e.value, '$.IdEntregable')),
            TRY_CONVERT(DATE, JSON_VALUE(e.value, '$.FechaAsociacion'))
        FROM OPENJSON(@Entregables) AS e;
    END;
END;
GO
CREATE OR ALTER PROCEDURE eliminar_producto_completo
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Producto WHERE Id = @IdProducto;
END;
GO
CREATE OR ALTER PROCEDURE actualizar_entregable_completo
    @IdEntregable INT,
    @Codigo NVARCHAR(50),
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX),
    @FechaInicio DATE,
    @FechaFinPrevista DATE,
    @FechaModificacion DATE = GETDATE(),
    @FechaFinalizacion DATE,
    @Actividades NVARCHAR(MAX) = NULL,  
    @Responsables NVARCHAR(MAX) = NULL,
    @Archivos NVARCHAR(MAX) = NULL     
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Entregable
    SET Codigo = @Codigo,
        Titulo = @Titulo,
        Descripcion = @Descripcion,
        FechaInicio = @FechaInicio,
        FechaFinPrevista = @FechaFinPrevista,
        FechaModificacion = @FechaModificacion,
        FechaFinalizacion = @FechaFinalizacion
    WHERE Id = @IdEntregable;
    DELETE FROM Actividad WHERE IdEntregable = @IdEntregable;

    IF @Actividades IS NOT NULL
    BEGIN
        INSERT INTO Actividad (IdEntregable, Titulo, Descripcion, FechaInicio, FechaFinPrevista, FechaModificacion, FechaFinalizacion, Prioridad, PorcentajeAvance)
        SELECT 
            @IdEntregable,
            JSON_VALUE(a.value, '$.Titulo'),
            JSON_VALUE(a.value, '$.Descripcion'),
            TRY_CONVERT(DATE, JSON_VALUE(a.value, '$.FechaInicio')),
            TRY_CONVERT(DATE, JSON_VALUE(a.value, '$.FechaFinPrevista')),
            TRY_CONVERT(DATE, JSON_VALUE(a.value, '$.FechaModificacion')),
            TRY_CONVERT(DATE, JSON_VALUE(a.value, '$.FechaFinalizacion')),
            TRY_CONVERT(INT, JSON_VALUE(a.value, '$.Prioridad')),
            TRY_CONVERT(INT, JSON_VALUE(a.value, '$.PorcentajeAvance'))
        FROM OPENJSON(@Actividades) AS a;
    END;
    DELETE FROM Responsable_Entregable WHERE IdEntregable = @IdEntregable;

    IF @Responsables IS NOT NULL
    BEGIN
        INSERT INTO Responsable_Entregable (IdResponsable, IdEntregable, FechaAsociacion)
        SELECT 
            TRY_CONVERT(INT, JSON_VALUE(r.value, '$.IdResponsable')),
            @IdEntregable,
            TRY_CONVERT(DATE, JSON_VALUE(r.value, '$.FechaAsociacion'))
        FROM OPENJSON(@Responsables) AS r;
    END;
    DELETE FROM Archivo_Entregable WHERE IdEntregable = @IdEntregable;

    IF @Archivos IS NOT NULL
    BEGIN
        INSERT INTO Archivo_Entregable (IdArchivo, IdEntregable)
        SELECT 
            TRY_CONVERT(INT, JSON_VALUE(f.value, '$.IdArchivo')),
            @IdEntregable
        FROM OPENJSON(@Archivos) AS f;
        SELECT @IdEntregable AS IdEntregable;
    END;
END;
GO
CREATE OR ALTER PROCEDURE eliminar_entregable_completo
    @IdEntregable INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Entregable WHERE Id = @IdEntregable;
END;
GO
CREATE OR ALTER PROCEDURE actualizar_presupuesto_completo
    @IdPresupuesto INT,
    @MontoSolicitado DECIMAL(15,2),
    @Estado NVARCHAR(20),
    @MontoAprobado DECIMAL(15,2),
    @PeriodoAnio INT,
    @FechaSolicitud DATE,
    @FechaAprobacion DATE,
    @Observaciones NVARCHAR(MAX),
    @Distribuciones NVARCHAR(MAX) = NULL, 
    @Ejecuciones NVARCHAR(MAX) = NULL   
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Presupuesto
    SET MontoSolicitado = @MontoSolicitado,
        Estado = @Estado,
        MontoAprobado = @MontoAprobado,
        PeriodoAnio = @PeriodoAnio,
        FechaSolicitud = @FechaSolicitud,
        FechaAprobacion = @FechaAprobacion,
        Observaciones = @Observaciones
    WHERE Id = @IdPresupuesto;

    DELETE FROM DistribucionPresupuesto WHERE IdPresupuestoPadre = @IdPresupuesto;
    DELETE FROM EjecucionPresupuesto WHERE IdPresupuesto = @IdPresupuesto;

    IF @Distribuciones IS NOT NULL
    BEGIN
        INSERT INTO DistribucionPresupuesto (IdPresupuestoPadre, IdProyectoHijo, MontoAsignado)
        SELECT 
            @IdPresupuesto,
            TRY_CONVERT(INT, JSON_VALUE(d.value, '$.IdProyectoHijo')),
            TRY_CONVERT(DECIMAL(15,2), JSON_VALUE(d.value, '$.MontoAsignado'))
        FROM OPENJSON(@Distribuciones) AS d;
    END;

    IF @Ejecuciones IS NOT NULL
    BEGIN
        INSERT INTO EjecucionPresupuesto (IdPresupuesto, Anio, MontoPlaneado, MontoEjecutado, Observaciones)
        SELECT 
            @IdPresupuesto,
            TRY_CONVERT(INT, JSON_VALUE(e.value, '$.Anio')),
            TRY_CONVERT(DECIMAL(15,2), JSON_VALUE(e.value, '$.MontoPlaneado')),
            TRY_CONVERT(DECIMAL(15,2), JSON_VALUE(e.value, '$.MontoEjecutado')),
            JSON_VALUE(e.value, '$.Observaciones')
        FROM OPENJSON(@Ejecuciones) AS e;
    END;
END;
GO
CREATE OR ALTER PROCEDURE eliminar_presupuesto_completo
    @IdPresupuesto INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Presupuesto WHERE Id = @IdPresupuesto;
END;
GO
CREATE OR ALTER PROCEDURE actualizar_variable_completa
    @IdVariable INT,
    @Titulo NVARCHAR(255),
    @Descripcion NVARCHAR(MAX),
    @Objetivos NVARCHAR(MAX) = NULL 
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE VariableEstrategica
    SET Titulo = @Titulo,
        Descripcion = @Descripcion
    WHERE Id = @IdVariable;

    DELETE FROM ObjetivoEstrategico WHERE IdVariable = @IdVariable;

    IF @Objetivos IS NOT NULL
    BEGIN
        DECLARE @IdObjetivo INT;
        DECLARE cur CURSOR FOR 
        SELECT value FROM OPENJSON(@Objetivos);

        OPEN cur;
        DECLARE @obj NVARCHAR(MAX);

        FETCH NEXT FROM cur INTO @obj;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO ObjetivoEstrategico (IdVariable, Titulo, Descripcion)
            VALUES (
                @IdVariable,
                JSON_VALUE(@obj, '$.Titulo'),
                JSON_VALUE(@obj, '$.Descripcion')
            );

            SET @IdObjetivo = SCOPE_IDENTITY();
            DECLARE @Metas NVARCHAR(MAX) = JSON_QUERY(@obj, '$.Metas');

            IF @Metas IS NOT NULL
            BEGIN
                INSERT INTO MetaEstrategica (IdObjetivo, Titulo, Descripcion)
                SELECT 
                    @IdObjetivo,
                    JSON_VALUE(m.value, '$.Titulo'),
                    JSON_VALUE(m.value, '$.Descripcion')
                FROM OPENJSON(@Metas) AS m;
            END;

            FETCH NEXT FROM cur INTO @obj;
        END;

        CLOSE cur;
        DEALLOCATE cur;
    END;
END;
GO
CREATE OR ALTER PROCEDURE eliminar_variable_completa
    @IdVariable INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM VariableEstrategica WHERE Id = @IdVariable;
END;
GO
CREATE OR ALTER PROCEDURE actualizar_entregable_con_actividades
    @p_id_entregable INT,
    @p_codigo NVARCHAR(50),
    @p_titulo NVARCHAR(255),
    @p_descripcion NVARCHAR(MAX),
    @p_fechainicio DATE = NULL,
    @p_fechafinprevista DATE = NULL,
    @p_fechamodificacion DATE = NULL,
    @p_fechafinalizacion DATE = NULL,
    @p_actividades NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        UPDATE Entregable
        SET Codigo = @p_codigo,
            Titulo = @p_titulo,
            Descripcion = @p_descripcion,
            FechaInicio = @p_fechainicio,
            FechaFinPrevista = @p_fechafinprevista,
            FechaModificacion = @p_fechamodificacion,
            FechaFinalizacion = @p_fechafinalizacion
        WHERE Id = @p_id_entregable;
        DELETE FROM Actividad
        WHERE IdEntregable = @p_id_entregable;
        INSERT INTO Actividad
        (
            IdEntregable, Titulo, Descripcion, FechaInicio, FechaFinPrevista,
            FechaModificacion, FechaFinalizacion, Prioridad, PorcentajeAvance
        )
        SELECT 
            @p_id_entregable,
            JSON_VALUE([value], '$.Titulo'),
            JSON_VALUE([value], '$.Descripcion'),
            TRY_CAST(JSON_VALUE([value], '$.FechaInicio') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.FechaFinPrevista') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.FechaModificacion') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.FechaFinalizacion') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.Prioridad') AS INT),
            TRY_CAST(JSON_VALUE([value], '$.PorcentajeAvance') AS INT)
        FROM OPENJSON(@p_actividades);

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE crear_entregable_con_actividades
    @p_codigo NVARCHAR(50),
    @p_titulo NVARCHAR(255),
    @p_descripcion NVARCHAR(MAX),
    @p_fechainicio DATE = NULL,
    @p_fechafinprevista DATE = NULL,
    @p_fechamodificacion DATE = NULL,
    @p_fechafinalizacion DATE = NULL,
    @p_actividades NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @nuevoIdEntregable INT;
        INSERT INTO Entregable
        (
            Codigo, Titulo, Descripcion, FechaInicio,
            FechaFinPrevista, FechaModificacion, FechaFinalizacion
        )
        VALUES
        (
            @p_codigo, @p_titulo, @p_descripcion,
            @p_fechainicio, @p_fechafinprevista,
            @p_fechamodificacion, @p_fechafinalizacion
        );

        SET @nuevoIdEntregable = SCOPE_IDENTITY();
        INSERT INTO Actividad
        (
            IdEntregable, Titulo, Descripcion, FechaInicio,
            FechaFinPrevista, FechaModificacion, FechaFinalizacion,
            Prioridad, PorcentajeAvance
        )
        SELECT 
            @nuevoIdEntregable,
            JSON_VALUE([value], '$.Titulo'),
            JSON_VALUE([value], '$.Descripcion'),
            TRY_CAST(JSON_VALUE([value], '$.FechaInicio') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.FechaFinPrevista') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.FechaModificacion') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.FechaFinalizacion') AS DATE),
            TRY_CAST(JSON_VALUE([value], '$.Prioridad') AS INT),
            TRY_CAST(JSON_VALUE([value], '$.PorcentajeAvance') AS INT)
        FROM OPENJSON(@p_actividades);

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE sp_ConsultarEntregablesConDetalles
    @IdEntregable INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT (
        SELECT 
            e.Id AS Id,
            e.Codigo,
            e.Titulo,
            e.Descripcion,
            e.FechaInicio,
            e.FechaFinPrevista,
            e.FechaModificacion,
            e.FechaFinalizacion,
            (
                SELECT 
                    a.Id AS IdActividad,
                    a.IdEntregable,
                    a.Titulo,
                    a.Descripcion,
                    a.FechaInicio,
                    a.FechaFinPrevista,
                    a.FechaModificacion,
                    a.FechaFinalizacion,
                    a.Prioridad,
                    a.PorcentajeAvance
                FROM Actividad a
                WHERE a.IdEntregable = e.Id
                FOR JSON PATH
            ) AS  DetalleActividades,
            (
                SELECT 
                    ar.Id AS IdArchivo,
                    ar.Nombre,
                    ar.Ruta,
                    ar.Tipo,
                    ar.Fecha,
                    ar.IdUsuario
                FROM Archivo_Entregable ae
                INNER JOIN Archivo ar ON ar.Id = ae.IdArchivo
                WHERE ae.IdEntregable = e.Id
                FOR JSON PATH
            ) AS DetalleArchivos

        FROM Entregable e
        WHERE e.Id = @IdEntregable
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    ) AS Json;
END;
GO
CREATE OR ALTER PROCEDURE sp_Archivo_Actualizar
    @Id INT,
    @IdUsuario INT,
    @Ruta NVARCHAR(MAX),
    @Nombre NVARCHAR(255),
    @Tipo NVARCHAR(50) = NULL,
    @Fecha DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Archivo WHERE Id = @Id)
    BEGIN
        RAISERROR('No se encontró un archivo con el Id especificado.', 16, 1);
        RETURN;
    END
    UPDATE Archivo
    SET 
        IdUsuario = @IdUsuario,
        Ruta = @Ruta,
        Nombre = @Nombre,
        Tipo = @Tipo,
        Fecha = @Fecha
    WHERE Id = @Id;
    SELECT 
        A.Id,
        A.IdUsuario,
        U.Email AS EmailUsuario,
        A.Ruta,
        A.Nombre,
        A.Tipo,
        A.Fecha
    FROM Archivo AS A
    INNER JOIN Usuario AS U ON U.Id = A.IdUsuario
    WHERE A.Id = @Id;
END;
GO
CREATE OR ALTER PROCEDURE sp_Archivo_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Archivo WHERE Id = @Id)
    BEGIN
        RAISERROR('No se encontró un archivo con el Id especificado.', 16, 1);
        RETURN;
    END
    DELETE FROM Archivo
    WHERE Id = @Id;
    SELECT 'Archivo eliminado correctamente.' AS Mensaje;
END;
GO
Create or alter procedure Buscar_Datos_Catalogo
	@Maestra nvarchar(50)
as 
Begin 
	if @Maestra = 'Productos' 
		BEGIN
			Select  Prod.Id, 
								Prod.IdTipoProducto, 
								Prod.Codigo,
								Prod.Titulo,
								Prod.Descripcion,
								Prod.FechaInicio, 
								Prod.FechaFinPrevista, 
								Prod.FechaModificacion, 
								Prod.FechaFinalizacion, 
								Prod.RutaLogo
			From Producto Prod 
			Inner Join TipoProducto TiPro
			On Prod.IdTipoProducto = TiPro.Id
			Order By Prod.Id;
		END
End; 
Go 
CREATE OR ALTER PROCEDURE eliminar_actividad
    @IdActividad INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Actividad WHERE Id = @IdActividad)
    BEGIN
        RAISERROR ('La actividad especificada no existe.', 16, 1);
        RETURN;
    END

    DELETE FROM Actividad
    WHERE Id = @IdActividad;
END;
GO
