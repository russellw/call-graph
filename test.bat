msbuild /p:Configuration=Debug /p:Platform="Any CPU"||exit /b
bin\Debug\net7.0\call-graph.exe %*||exit /b
bat bin\call-graph-call-graph.html
