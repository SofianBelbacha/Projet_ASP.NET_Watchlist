# 🎬 Watchlist

Watchlist est une application ASP.NET Core permettant aux utilisateurs de gérer et suivre leurs films préférés, avec des fonctionnalités de recherche, tri et notation.

## 🚀 Fonctionnalités

- 🔍 **Recherche avancée** : Filtrer les films par titre, année, ou notes.
- 🎭 **Gestion des films** : Ajouter des films à une liste personnelle, les marquer comme vus et leur attribuer une note.
- 📊 **Tri personnalisé** : Trier par année, titre ou note.
- 🔐 **Authentification** : Inscription et connexion sécurisées.
  
## 🛠️ Technologies utilisées

- **Backend** : ASP.NET Core, Entity Framework Core
- **Frontend** : Bootstrap, Razor Pages
- **Base de données** : SQL Server
- **Authentification** : Identity Framework

## 📦 Installation

1. **Cloner le projet**
   ```sh
   git clone https://github.com/ton-utilisateur/watchlist.git
   cd watchlist
   ```

2. **Configurer la base de données**
   - Changer `appsettings.example.json` pour `appsettings.json`
   - Modifier `appsettings.json` pour mettre à jour la chaîne de connexion.
   - Appliquer les migrations :
     ```sh
     dotnet ef database update
     ```

3. **Lancer l'application**
   ```sh
   dotnet run
   ```

## 📄 Structure du projet

```
📂 Watchlist
│-- 📂 Controllers       # Gestion des routes et de la logique métier
│-- 📂 Models            # Définition des classes de données
│-- 📂 Views             # Pages Razor et composants UI
│-- 📂 Services          # Services partagés (ex: FilmService)
│-- 📂 Data              # Configuration de la base de données
│-- appsettings.json     # Configuration principale
│-- Program.cs          # Point d'entrée de l'application
```

## 🛠️ Contribution

Les contributions sont les bienvenues ! 🚀
- **Fork** le projet
- **Crée une branche** (`git checkout -b feature/ma-nouvelle-fonctionnalité`)
- **Fais tes modifications** et commit (`git commit -m 'Ajout d'une nouvelle fonctionnalité'`)
- **Pousse la branche** (`git push origin feature/ma-nouvelle-fonctionnalité`)
- **Ouvre une Pull Request** 📌

---
✨ **Développé avec passion par Sofian Belbacha** ✨

