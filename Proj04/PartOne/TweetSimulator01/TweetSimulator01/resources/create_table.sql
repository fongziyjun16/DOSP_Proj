﻿CREATE TABLE ACCOUNT (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    NAME TEXT NOT NULL
);

CREATE TABLE FOLLOW (
    NAME TEXT NOT NULL,
    FOLLOWER TEXT NOT NULL, -- follower name
    PRIMARY KEY (NAME, FOLLOWER)
);

CREATE TABLE HASHTAG (
     ID INTEGER PRIMARY KEY AUTOINCREMENT,
     TOPIC TEXT NOT NULL UNIQUE,
     CREATOR TEXT NOT NULL -- name
);

CREATE TABLE TWEET (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    CREATOR TEXT NOT NULL, -- name
    CONTENT TEXT NOT NULL,
    RETWEETID INTEGER
);

CREATE TABLE TWEET_MENTION (
    TWEETID INTEGER NOT NULL,
    NAME TEXT NOT NULL,
    PRIMARY KEY (TWEETID, NAME)
);

CREATE TABLE TWEET_HASHTAG (
    TWEETID INTEGER NOT NULL,
    HASHTAGID INTEGER NOT NULL,
    PRIMARY KEY (TWEETID, HASHTAGID)
);