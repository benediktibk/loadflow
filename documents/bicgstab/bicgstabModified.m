function x = bicgstabModified(A, x, b, M, max_it, tol)
  r = b - A*x;
  omega = 1.0;
  alpha = 1.0;
  rFirst = r;

  for i = 1:max_it  
     rho = (rFirst'*r);             

     if ( i > 1 ),
        beta  = (rho/rhoPrevious)*(alpha/omega);
        p = r + beta*(p - omega*v);
     else
        p = r;
     end
 
     p_hat = M \ p;
     v = A*p_hat;
     alpha = rho/(rFirst'*v);
     s = r - alpha*v;
     s_hat = M \ s;                                 
     t = A*s_hat;
     omega = (t'*s)/(t'*t);
     x = x + alpha*p_hat + omega*s_hat;     
     rhoPrevious = rho;
     r = s - omega*t;
     
     error = norm(r);                     
     if (error <= tol)
         break
     end
  end
end