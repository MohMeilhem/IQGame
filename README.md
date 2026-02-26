# IQ Game - .NET Web Application

A competitive, team-based Q&A platform designed to facilitate engaging educational and social events. The application allows users to manage game sessions, select categories, and track scores in real-time with strategic gameplay elements.

## Features
- **Dynamic Session Management:** Create and configure game sessions with custom team names.
- **Categorized Challenges:** Select from a pool of 50+ categories, each containing tiered difficulty levels.
- **Advanced Game Logic:** Includes a dual-timer system (60s/30s) and strategic power-ups (Double Points, Two Answers, Multiple Choice).
- **Responsive UI:** Fully optimized for both desktop and mobile web browsers.

## Tech Stack
- **Backend:** .NET Core, C#
- **Database:** SQL Server (Relational Schema with 8+ tables)
- **Frontend:** HTML5, CSS3, JavaScript
- **Data Handling:** JSON for dynamic question options

## Database Architecture
The system relies on a normalized RDBMS structure to manage:
- User authentication and session tracking.
- Many-to-Many relationships between sessions and categories.
- Detailed logging of question attempts and power-up usage.
