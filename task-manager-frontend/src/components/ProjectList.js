import React, { useEffect, useState } from "react";
import { getProjects } from "../api";

function ProjectList() {
  const [projects, setProjects] = useState([]);

  useEffect(() => {
    getProjects()
      .then(res => setProjects(res.data))
      .catch(err => console.error(err));
  }, []);

  return (
    <div>
      <h2>Projects</h2>
      <ul>
        {projects.map(project => (
          <li key={project.id}>
            <strong>{project.name}</strong> - {project.description}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default ProjectList;
