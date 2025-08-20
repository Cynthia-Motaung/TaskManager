import axios from "axios";

const API_URL = "https://localhost:5131/api"; // base URL for your API


export const getProjects = () => axios.get(`${API_URL}/projects`);
export const getTasks = () => axios.get(`${API_URL}/tasks`);
export const getUsers = () => axios.get(`${API_URL}/users`);
export const getTaskAssignments = () => axios.get(`${API_URL}/taskassignments`);
