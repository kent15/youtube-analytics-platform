-- YouTube Analytics Tool - Initial Schema

CREATE TABLE IF NOT EXISTS channels (
    channel_id VARCHAR(50) PRIMARY KEY,
    channel_name VARCHAR(500) NOT NULL,
    subscriber_count BIGINT NOT NULL DEFAULT 0,
    total_view_count BIGINT NOT NULL DEFAULT 0,
    video_count BIGINT NOT NULL DEFAULT 0,
    uploads_playlist_id VARCHAR(50) NOT NULL,
    retrieved_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS videos (
    video_id VARCHAR(50) PRIMARY KEY,
    channel_id VARCHAR(50) NOT NULL REFERENCES channels(channel_id),
    title VARCHAR(1000) NOT NULL,
    published_at TIMESTAMP WITH TIME ZONE NOT NULL,
    view_count BIGINT NOT NULL DEFAULT 0,
    like_count BIGINT NOT NULL DEFAULT 0,
    comment_count BIGINT NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_videos_channel_id ON videos(channel_id);
CREATE INDEX IF NOT EXISTS idx_videos_published_at ON videos(published_at);
