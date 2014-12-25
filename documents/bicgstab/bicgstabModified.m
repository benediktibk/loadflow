function x = bicgstabModified(A, x, b, M, max_it, tol)
  r = b - A*x;
  omega = 1.0;
  alpha = 1.0;
  r_tld = r;

  for iter = 1:max_it,                              

     rho = (r_tld'*r);             

     if ( iter > 1 ),
        beta  = ( rho/rho_1 )*( alpha/omega );
        p = r + beta*( p - omega*v );
     else
        p = r;
     end
 
     p_hat = M \ p;
     v = A*p_hat;
     alpha = rho / ( r_tld'*v );
     s = r - alpha*v;
     if ( norm(s) < tol ),                          
        x = x + alpha*p_hat;
        resid = norm(s);
        break;
     end

     s_hat = M \ s;                                 
     t = A*s_hat;
     omega = ( t'*s) / ( t'*t );

     x = x + alpha*p_hat + omega*s_hat;             

     r = s - omega*t;
     error = norm(r);                     
     if ( error <= tol ), break, end

     if ( omega == 0.0 ), break, end
     rho_1 = rho;

  end
end