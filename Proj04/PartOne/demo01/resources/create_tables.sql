CREATE TABLE ACCOUNT (
    USERNAME TEXT PRIMARY KEY,
    PASSWORD TEXT NOT NULL,
    NAME TEXT NOT NULL UNIQUE,
    GENDER TEXT -- male / female
);

CREATE TABLE FOLLOW (
    USERNAME TEXT NOT NULL,
    FOLLOWER TEXT NOT NULL, -- follower username
    PRIMARY KEY (USERNAME, FOLLOWER) 
);

CREATE TABLE HASHTAG (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    TOPIC TEXT NOT NULL UNIQUE,
    CREATOR TEXT NOT NULL -- username
);

CREATE TABLE TWEET (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    CREATOR TEXT NOT NULL, -- username
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