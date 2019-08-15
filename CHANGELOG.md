## 0.1.9 (15.08.2019)

Potential fix for https://github.com/vostok/clusterclient.core/issues/6

## 0.1.8 (17.07.2019)

WeighedReplicaOrdering now builds its internal segment tree in O(N) time instead of O(N * log(N)).

## 0.1.7 (29.04.2019):

Added support for asynchronous request transforms.

## 0.1.6 (23.04.2019):

Removed redundant trailing 's' from 'Request-Timeout' header value format.

## 0.1.5 (15-03-2019):

Fixed https://github.com/vostok/clusterclient.core/issues/3

## 0.1.4 (03-03-2019): 

Introduced support for request bodies consisting of multiple buffer segments.

## 0.1.3 (16-02-2019): 

Enchancements in auxiliary headers modules.

## 0.1.2 (16-02-2019): 

AddRequestModule() extension is now idempotent.

## 0.1.1 (16-02-2019): 

Minor changes: added context-related header names, changed default request priority to null.

## 0.1.0 (04-02-2019): 

Initial prerelease.