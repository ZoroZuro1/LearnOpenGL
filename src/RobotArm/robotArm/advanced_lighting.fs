#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

struct Light {
    vec3 direction;
    float cutOff;
    float outerCutOff;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
	
    float constant;
    float linear;
    float quadratic;
};

uniform sampler2D floorTexture;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec3 lightColor;
uniform vec3 ObjColor;
uniform Light light;
uniform mat4 model;
uniform bool lKey;

void main()
{          
    vec3 flagPosWorld = vec3(model * vec4(fs_in.FragPos, 1.0));
    vec3 color = texture(floorTexture, fs_in.TexCoords).rgb;

    // ambient
    vec3 ambient = 0.1 *  color;

    // diffuse
    vec3 lightDir = normalize(lightPos - flagPosWorld);
    vec3 normal = normalize(fs_in.Normal);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = light.diffuse * diff * color;
    
    // specular
    vec3 viewDir = normalize(viewPos - flagPosWorld);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = 0.0;
    spec = pow(max(dot(normal, reflectDir), 0.0), 32); //32
    vec3 specular = light.specular * lightColor * spec; 
    
    float intensity = 1.0f; // 기본값

    // spotlight (soft edges)
    if(lKey){
        float theta = dot(lightDir, normalize(-light.direction)); 
        float epsilon = (light.cutOff - light.outerCutOff);
        intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
        diffuse *= intensity;
        specular *= intensity;
    }
    
    // attenuation
    float distance = length(lightPos - flagPosWorld);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    ambient *= attenuation; 
    diffuse *= attenuation;
    specular *= attenuation; 
    
    FragColor = vec4(ambient + diffuse + specular, 1.0);
}