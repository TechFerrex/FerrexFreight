# Sistema de Roles con Tabla en Base de Datos

## Descripcion General

Se migro el sistema de roles de un campo `string` libre en la tabla `Users` a una **tabla independiente `Roles`** con relacion FK. Esto garantiza que solo se asignen roles validos y permite gestionar los roles desde la base de datos.

---

## Antes vs Despues

| Aspecto | Antes | Despues |
|---------|-------|---------|
| Almacenamiento | `string Role` en tabla Users | `int RoleId` FK a tabla Roles |
| Validacion | Ninguna (texto libre) | FK constraint, solo roles existentes |
| Roles disponibles | Hardcodeados en codigo | Cargados desde BD |
| Agregar un rol nuevo | Cambiar codigo en multiples archivos | INSERT en tabla Roles |

---

## Modelo de Datos

### Role (`Models/Role.cs`)

| Propiedad | Tipo | Descripcion |
|-----------|------|-------------|
| `Id` | `int` [Key] | Clave primaria |
| `Name` | `string` [Required, MaxLength(50)] | Nombre del rol |
| `Users` | `ICollection<User>` | Usuarios con este rol |

### Roles Seed (datos iniciales)

| Id | Name | Descripcion |
|----|------|-------------|
| 1 | User | Cliente/usuario normal |
| 2 | Admin | Administrador |
| 3 | SuperAdmin | Super administrador (acceso total) |
| 4 | Worker | Empleado/trabajador |

### User (`Models/User.cs`) - Cambios

```csharp
// Se elimino:
public string Role { get; set; } = "User";

// Se agrego:
public int RoleId { get; set; }
public Role Role { get; set; }  // Navigation property
```

---

## Relacion en ApplicationDbContext

```csharp
// DbSet
public DbSet<Role> Roles { get; set; }

// Seed data
modelBuilder.Entity<Role>().HasData(
    new Role { Id = 1, Name = "User" },
    new Role { Id = 2, Name = "Admin" },
    new Role { Id = 3, Name = "SuperAdmin" },
    new Role { Id = 4, Name = "Worker" }
);

// Relacion
modelBuilder.Entity<User>()
    .HasOne(u => u.Role)
    .WithMany(r => r.Users)
    .HasForeignKey(u => u.RoleId);
```

---

## Migracion: `AddRolesTable`

Archivo: `Migrations/20260315025401_AddRolesTable.cs`

Orden de ejecucion:
1. Crear tabla `Roles`
2. Insertar los 4 roles iniciales (seed)
3. Agregar columna `RoleId` a `Users` con default 3 (SuperAdmin)
4. `UPDATE Users SET RoleId = 3` (usuarios existentes → SuperAdmin)
5. Eliminar columna vieja `Role` (string)
6. Crear indice y FK constraint

---

## Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `Models/Role.cs` | **Nuevo** - Modelo de la tabla Roles |
| `Models/User.cs` | `string Role` → `int RoleId` + navegacion `Role` |
| `Services/ApplicationDbContext.cs` | `DbSet<Role>`, relacion, seed data |
| `Pages/UserInfo.razor` | Login: `Include(u => u.Role)`, Claims: `user.Role?.Name` |
| `Pages/UserManagement.razor` | Roles desde BD, dropdown con `RoleId`, display `Role?.Name` |

---

## Uso en Codigo

### Obtener el nombre del rol de un usuario
```csharp
// Siempre incluir la navegacion al consultar
var user = await DbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(...);
string roleName = user.Role?.Name ?? "User";
```

### Asignar rol al crear usuario
```csharp
var user = new User { RoleId = 1 }; // 1=User, 2=Admin, 3=SuperAdmin, 4=Worker
```

### Autorizacion en paginas Razor (sin cambios)
```razor
@attribute [Authorize(Roles = "SuperAdmin")]
```
Sigue funcionando porque el claim se genera con `user.Role.Name`.

### Cargar roles disponibles desde BD
```csharp
var roles = await DbContext.Roles.ToListAsync();
```
