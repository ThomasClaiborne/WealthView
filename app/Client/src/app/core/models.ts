export interface AppUser {
    appUserId: number;
    username:  string;
    email:     string;
    firstName: string;
    lastName:  string;
}

export interface AuthResponse {
    token:     string;
    appUserId: number;
    username:  string;
    email:     string;
    firstName: string;
    lastName:  string;
}