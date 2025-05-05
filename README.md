# Scheduling Desktop WGU
Scheduling Desktop Application – Developer Documentation
 Overview
This C# WPF desktop application was developed to meet real-world scheduling and customer management requirements for a global consulting firm. The application connects to a MySQL backend and provides functionality for user login, customer and appointment management, reporting, localization, time zone handling, and login tracking.
________________________________________
 Project Structure
SchedulingDesktopWGU/
├── App.xaml
├── DBHelper.cs
├── MainWindow.xaml             → Login Window
├── Views/
│   ├── CustomerForm.xaml       → Add/Update/Delete Customers
│   ├── AppointmentForm.xaml    → Schedule Appointments
│   ├── CalendarView.xaml       → View Appointments by Day
│   ├── ReportsWindow.xaml      → Run Summary Reports
├── Resources/
│   ├── Strings.en.xaml         → English UI Strings
│   ├── Strings.es.xaml         → Spanish UI Strings
├── Login_History.txt           → Auto-generated on successful login
________________________________________
 Login (MainWindow.xaml)
•	Validates the username and password against the user table.
•	Automatically detects system language (English or Spanish) and translates messages accordingly.
•	Logs every successful login with a timestamp and username in Login_History.txt.
•	Triggers a check for appointments within the next 15 minutes and notifies the user.
 SQL location: MainWindow.xaml.cs
Uses parameterized queries to avoid SQL injection.
________________________________________
 Customer Management (CustomerForm.xaml)
•	Allows the user to add, update, and delete customers.
•	Validates name, address, and phone input (only digits and dashes allowed).
•	Uses joined queries to update both customer and related address tables.
•	Displays current customers in a DataGrid with real-time updates.
SQL location: CustomerForm.xaml.cs
 Uses collection binding and selection change handlers.
________________________________________
 Appointment Scheduling (AppointmentForm.xaml)
•	Users can add, update, and delete appointments.
•	Appointments are required to be within business hours (9 AM–5 PM EST, M–F).
•	Overlapping appointment logic is handled before saving.
•	Appointment times are stored in UTC, and converted to local time using .ToLocalTime().
SQL location: AppointmentForm.xaml.cs
Uses DateTime.ToUniversalTime() and ToLocalTime() for global accuracy.
________________________________________
 Calendar View (CalendarView.xaml)
•	Provides a visual calendar to view appointments on a specific day.
•	Selecting a date loads all appointments for that day, converted to the user’s local time zone.
SQL location: CalendarView.xaml.cs
Queries by start BETWEEN UTC range and displays with ToLocalTime().
________________________________________
 Reports (ReportsWindow.xaml)
•	Offers 3 reports using collection classes and lambda expressions:
1.	Appointments by type and month
2.	Schedules grouped by user
3.	Appointments by customer city
•	Users select the report type via a dropdown menu.
SQL location: ReportsWindow.xaml.cs
All reports use DataTable.AsEnumerable() + GroupBy()/Select() with lambda expressions.
________________________________________
Localization and Globalization
•	The application uses resource dictionaries (Strings.en.xaml, Strings.es.xaml) for multilingual support.
•	All messages on the login screen are localized based on the detected culture (e.g., es-ES).
________________________________________
Time Zone & DST Handling
•	Appointment times are stored in UTC in the MySQL database.
•	All time-related displays in the app are converted with .ToLocalTime(), which accounts for daylight saving time automatically.
________________________________________
 Advanced Features Used
•	Lambda expressions for collection processing
•	Resource dictionaries for localization
•	TimeZoneInfo and DateTime methods for UTC conversion
•	Exception handling for all DB interactions
•	Data validation using Regex and string trimming
•	File handling for persistent login tracking
________________________________________ Developer Notes
•	All SQL queries are scoped within their respective view's code-behind (no centralized data access layer was required for this assessment).
•	The database schema is based on WGU’s provided ERD and includes proper foreign key relationships.
•	No external libraries were used — only the .NET Framework and MySQL Connector.
________________________________________
