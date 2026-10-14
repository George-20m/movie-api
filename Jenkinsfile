pipeline {
  agent any
  options {
    timestamps()
    disableConcurrentBuilds()
  }
  triggers {
    pollSCM('H/5 * * * *')
  }
  stages {
    stage('Test') {
      steps {
        sh 'docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e DOTNET_NOLOGO=1 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test tests/MovieApi.Tests/MovieApi.Tests.csproj -c Release'
      }
    }
    stage('Deploy') {
      steps {
        withCredentials([file(credentialsId: 'movie-env', variable: 'ENV_FILE')]) {
          sh '''
            cp "$ENV_FILE" .env
            docker compose up -d --build
            rm -f .env
          '''
        }
      }
    }
    stage('Smoke test') {
      steps {
        sh '''
          for i in $(seq 1 40); do
            curl -fsS http://127.0.0.1:5000/health && exit 0
            sleep 3
          done
          exit 1
        '''
      }
    }
  }
}
