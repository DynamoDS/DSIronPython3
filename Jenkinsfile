#!/usr/bin/env groovy

@Library('PSL@master')
@Library('CILibrary@feature/sonarscanner-dotnet') _

StartPipeline()

@NonCPS
def printParams() {
  env.getEnvironment().each { name, value -> println "$name : $value" }
}
println "*******************************************************************"
printParams()
println "*******************************************************************"
