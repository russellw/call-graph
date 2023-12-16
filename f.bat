clang-format -i --style=file *.cs||exit /b
call clean-cs -i -r .||exit /b
clang-format -i --style=file *.cs||exit /b
git diff
