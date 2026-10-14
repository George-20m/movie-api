# Usage: source scripts/dev-env.sh
# Exports the Oracle connection string for local development (reads the password from ~/oracle.env).
export DB_PASSWORD="$(grep '^APP_USER_PASSWORD=' "$HOME/oracle.env" | cut -d= -f2)"
export ConnectionStrings__Oracle="User Id=movies_app;Password=${DB_PASSWORD};Data Source=127.0.0.1:1521/FREEPDB1"
