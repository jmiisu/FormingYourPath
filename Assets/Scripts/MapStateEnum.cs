using UnityEngine;

public enum MAP_STATE
{
    EMPTY,          // 빈 곳
    STAGE_BLOCK,    // 스테이지 고정 블록
    BASIC,          // 기본 블록
    STAIR,          // 계단 블록
    EXIT,           // 출구
    PLAYER_POS,     // 플레이어 위치
};

/*
 * 0 0 0 0 0 0 
 * 0 0 0 0 0 0 
 * 0 0 0 0 0 0
 * 1 0 2 P 3 2
 * 1 1 1 1 1 1
 */