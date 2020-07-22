# Minesweeper

#Generate database migration

```bash

dotnet ef database drop --project DAL --startup-project WebApp
dotnet ef migrations --project DAL --startup-project WebApp add InitialDbCreation 
dotnet ef migrations --project DAL --startup-project WebApp remove
dotnet ef database update --project DAL --startup-project WebApp
```

