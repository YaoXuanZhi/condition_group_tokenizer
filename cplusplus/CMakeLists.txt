cmake_minimum_required(VERSION 3.20)
project(cplusplus)

set(CMAKE_CXX_STANDARD 98)

add_executable(cplusplus main.cpp ConditionGroupTokenizer.cpp ConditionGroupTokenizer.h)
