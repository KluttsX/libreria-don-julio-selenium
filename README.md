
# Librería Don Julio - Selenium

Proyecto de pruebas automatizadas con Selenium y C# sobre una aplicación web para la gestión de libros, desarrollada con ASP.NET Core MVC, Entity Framework Core y SQLite.

## Funcionalidades

- Inicio de sesión.
- Registrar libros.
- Consultar libros.
- Editar libros.
- Eliminar libros.

## Pruebas automatizadas

El proyecto cuenta con 15 escenarios de prueba distribuidos en cinco historias de usuario. Cada historia incluye un camino feliz, una prueba negativa y una prueba de límites.

## Requisitos

- .NET SDK 10.0.
- Entity Framework Core CLI (`dotnet-ef`), versión 10.0.15 recomendada.
- Google Chrome.

## Instalación

1. Clonar el repositorio:

   ```bash
   git clone https://github.com/KluttsX/libreria-don-julio-selenium.git
   ```

2. Acceder al repositorio:

   ```bash
   cd libreria-don-julio-selenium
   ```

3. Restaurar las dependencias:

   ```bash
   dotnet restore
   ```

4. Verificar si Entity Framework Core CLI está instalado:

   ```bash
   dotnet ef --version
   ```

   En caso de que la herramienta no esté instalada o el comando no sea reconocido, ejecutar:

   ```bash
   dotnet tool install --global dotnet-ef --version 10.0.15
   ```

   Si ya se tiene instalada otra versión de `dotnet-ef` y funciona correctamente, no es necesario cambiarla.

   Únicamente en caso de presentar errores de compatibilidad al aplicar las migraciones, desinstalar la versión actual e instalar la versión correspondiente al proyecto (10.0.15):

   ```bash
   dotnet tool uninstall --global dotnet-ef
   dotnet tool install --global dotnet-ef --version 10.0.15
   ```

## Ejecutar la aplicación

1. Acceder al proyecto:

   ```bash
   cd src/LibreriaDonJulio
   ```

2. Aplicar las migraciones:

   ```bash
   dotnet ef database update
   ```

3. Ejecutar la aplicación:

   ```bash
   dotnet run
   ```

4. Abrir en el navegador la URL indicada en la terminal.

### Credenciales de prueba

Las credenciales de prueba están configuradas por defecto en el archivo `src/LibreriaDonJulio/appsettings.json`.

- Usuario: `Tuco`
- Contraseña: `thebest2026`

## Ejecutar las pruebas

Desde la raíz del repositorio, ejecutar:

```bash
dotnet test
```

Las pruebas utilizan una base de datos temporal para evitar modificar los registros de la aplicación principal. No es necesario iniciar la aplicación manualmente antes de ejecutar las pruebas.

## Reporte de resultados

Al finalizar las pruebas, se genera un reporte HTML con los resultados, el tiempo de ejecución y una captura de pantalla de cada escenario.

El reporte se encuentra en:

`test/LibreriaDonJulio.Tests/TestResults/latest/index.html`
