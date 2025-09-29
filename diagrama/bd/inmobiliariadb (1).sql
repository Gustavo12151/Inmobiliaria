-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 29-09-2025 a las 23:51:52
-- Versión del servidor: 10.4.28-MariaDB
-- Versión de PHP: 8.2.4

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `inmobiliariadb`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `contratos`
--

CREATE TABLE `contratos` (
  `Id` int(11) NOT NULL,
  `InmuebleId` int(11) NOT NULL,
  `InquilinoId` int(11) NOT NULL,
  `FechaInicio` date NOT NULL,
  `FechaFin` date NOT NULL,
  `FechaTerminacionAnticipada` date DEFAULT NULL,
  `MontoMensual` decimal(10,2) NOT NULL,
  `MultaCalculada` decimal(10,2) DEFAULT NULL,
  `UsuarioCreadorId` int(11) NOT NULL,
  `UsuarioFinalizadorId` int(11) DEFAULT NULL,
  `EstadoContrato` varchar(20) NOT NULL DEFAULT 'Vigente'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `contratos`
--



-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inmuebles`
--

CREATE TABLE `inmuebles` (
  `Id` int(11) NOT NULL,
  `Direccion` varchar(100) NOT NULL,
  `Ambientes` int(11) NOT NULL,
  `Superficie` decimal(10,2) NOT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `Estado` enum('Disponible','Ocupado') NOT NULL DEFAULT 'Disponible',
  `PropietarioId` int(11) NOT NULL,
  `TipoInmuebleId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `inmuebles`
--

INSERT INTO `inmuebles` (`Id`, `Direccion`, `Ambientes`, `Superficie`, `Precio`, `Estado`, `PropietarioId`, `TipoInmuebleId`) VALUES
(1, 'Av. San Martín 123', 4, 75.50, 55000.00, 'Disponible', 1, 1),
(2, 'Belgrano 456', 4, 120.00, 95000.00, 'Disponible', 2, 2),
(4, 'Belgrano 890', 5, 400.00, 56000.00, 'Disponible', 5, 2),
(6, 'Pedernera 1500', 2, 100.00, 32000.00, 'Disponible', 2, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inquilinos`
--

CREATE TABLE `inquilinos` (
  `Id` int(11) NOT NULL,
  `DNI` varchar(20) NOT NULL,
  `Nombre` varchar(50) NOT NULL,
  `Apellido` varchar(50) NOT NULL,
  `Contacto` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `inquilinos`
--

INSERT INTO `inquilinos` (`Id`, `DNI`, `Nombre`, `Apellido`, `Contacto`) VALUES
(1, '29345000', 'Carlos', 'López', 'carlos.lopez@mail.com'),
(2, '30456789', 'Laura', 'Martínez', 'laura.martinez@mail.com'),
(3, '12456987', 'Monica', 'Diaz', 'monidiaz@diaz'),
(4, '32456789', 'Luis', 'Flores', 'flores@f');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pagos`
--

CREATE TABLE `pagos` (
  `Id` int(11) NOT NULL,
  `ContratoId` int(11) NOT NULL,
  `NumeroPago` int(11) NOT NULL,
  `FechaPago` date NOT NULL,
  `Importe` decimal(10,2) NOT NULL,
  `Concepto` varchar(255) DEFAULT NULL,
  `UsuarioCreadorId` int(11) NOT NULL,
  `UsuarioAnuladorId` int(11) DEFAULT NULL,
  `TipoPago` varchar(20) NOT NULL DEFAULT 'Alquiler',
  `Estado` varchar(20) NOT NULL DEFAULT 'Activo'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `pagos`
--



-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `propietarios`
--

CREATE TABLE `propietarios` (
  `Id` int(11) NOT NULL,
  `DNI` varchar(20) NOT NULL,
  `Nombre` varchar(50) NOT NULL,
  `Apellido` varchar(50) NOT NULL,
  `Contacto` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `propietarios`
--

INSERT INTO `propietarios` (`Id`, `DNI`, `Nombre`, `Apellido`, `Contacto`) VALUES
(1, '30123456', 'Juan', 'Pérez', 'juan.perez@mail.com'),
(2, '30234567', 'Ana', 'Gómez', 'ana.gomez@mail.com'),
(3, '29345679', 'Carlos', 'López', 'carlos.lopez@mail.com'),
(4, '78456123', 'Juan', 'Perez', 'Jp@jp'),
(5, '45678900', 'Pepe', 'Ruiz', 'ruiz@tuiz');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tiposinmuebles`
--

CREATE TABLE `tiposinmuebles` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `tiposinmuebles`
--

INSERT INTO `tiposinmuebles` (`Id`, `Nombre`) VALUES
(1, 'Departamento'),
(2, 'Casa'),
(3, 'Local Comercial');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `Id` int(11) NOT NULL,
  `NombreUsuario` varchar(50) NOT NULL,
  `Clave` varchar(255) NOT NULL,
  `Rol` enum('Administrador','Empleado') NOT NULL DEFAULT 'Empleado',
  `Avatar` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`Id`, `NombreUsuario`, `Clave`, `Rol`, `Avatar`) VALUES
(1, 'admin', '$2a$11$aOyzul4O2MJWnux55YKAeeEF7gc5C0bg6mGNM3gXGHSxaUyT44p4a', 'Administrador', '/avatars/b1366b0c-7a46-40af-8578-67ab8c6f1aa9.png'),
(2, 'empleado2', '$2a$11$XfhvAE2ZzhBbokP3kICPc.vCwVAmmPnYoc0XaCWXHwY/Dahs1ceIu', 'Empleado', '/avatars/5a0b763a-1ace-4295-b89f-d05f8218ff52.png');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `contratos`
--
ALTER TABLE `contratos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `InmuebleId` (`InmuebleId`),
  ADD KEY `InquilinoId` (`InquilinoId`),
  ADD KEY `UsuarioCreadorId` (`UsuarioCreadorId`),
  ADD KEY `UsuarioFinalizadorId` (`UsuarioFinalizadorId`);

--
-- Indices de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PropietarioId` (`PropietarioId`),
  ADD KEY `TipoInmuebleId` (`TipoInmuebleId`);

--
-- Indices de la tabla `inquilinos`
--
ALTER TABLE `inquilinos`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `DNI` (`DNI`);

--
-- Indices de la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `ContratoId` (`ContratoId`),
  ADD KEY `UsuarioCreadorId` (`UsuarioCreadorId`),
  ADD KEY `UsuarioAnuladorId` (`UsuarioAnuladorId`);

--
-- Indices de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  ADD PRIMARY KEY (`Id`);

--
-- Indices de la tabla `tiposinmuebles`
--
ALTER TABLE `tiposinmuebles`
  ADD PRIMARY KEY (`Id`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `NombreUsuario` (`NombreUsuario`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `contratos`
--
ALTER TABLE `contratos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `inquilinos`
--
ALTER TABLE `inquilinos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `pagos`
--
ALTER TABLE `pagos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `tiposinmuebles`
--
ALTER TABLE `tiposinmuebles`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `contratos`
--
ALTER TABLE `contratos`
  ADD CONSTRAINT `contratos_ibfk_1` FOREIGN KEY (`InmuebleId`) REFERENCES `inmuebles` (`Id`),
  ADD CONSTRAINT `contratos_ibfk_2` FOREIGN KEY (`InquilinoId`) REFERENCES `inquilinos` (`Id`),
  ADD CONSTRAINT `contratos_ibfk_3` FOREIGN KEY (`UsuarioCreadorId`) REFERENCES `usuarios` (`Id`),
  ADD CONSTRAINT `contratos_ibfk_4` FOREIGN KEY (`UsuarioFinalizadorId`) REFERENCES `usuarios` (`Id`);

--
-- Filtros para la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  ADD CONSTRAINT `inmuebles_ibfk_1` FOREIGN KEY (`PropietarioId`) REFERENCES `propietarios` (`Id`),
  ADD CONSTRAINT `inmuebles_ibfk_2` FOREIGN KEY (`TipoInmuebleId`) REFERENCES `tiposinmuebles` (`Id`);

--
-- Filtros para la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD CONSTRAINT `pagos_ibfk_1` FOREIGN KEY (`ContratoId`) REFERENCES `contratos` (`Id`),
  ADD CONSTRAINT `pagos_ibfk_2` FOREIGN KEY (`UsuarioCreadorId`) REFERENCES `usuarios` (`Id`),
  ADD CONSTRAINT `pagos_ibfk_3` FOREIGN KEY (`UsuarioAnuladorId`) REFERENCES `usuarios` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
