# Vostok.ClusterClient.Core

[![Build status](https://ci.appveyor.com/api/projects/status/github/vostok/clusterclient.core?svg=true&branch=master)](https://ci.appveyor.com/project/vostok/clusterclient.core/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Vostok.ClusterClient.Core.svg)](https://www.nuget.org/packages/Vostok.ClusterClient.Core)

A library with interfaces and implementation of ClusterClient.

ClusterClient is a HTTP client which simplifies development of typical API clients.
It allows you to send a requests to cluster of replicas.

ClusterClient helps to organize fault-tolerant work with HTTP requests
using request sending strategies, retry strategies, configurable delays,
replicas reordering and some other approaches.
